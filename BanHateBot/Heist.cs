using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BanHateBot.Models;
using TwitchLib.Client;

namespace BanHateBot
{
    public class Heist
    {
        #region Static Properties
        private static DateTimeOffset? LastHeistStart { get; set; }
        private static DateTimeOffset? LastHeistEnd { get; set; }
        #endregion
        #region Fields
        private string _channelName;
        private CancellationTokenSource _cts;
        #endregion

        #region HeistSettings
        public HeistSettings HeistSettings { get; set; }
        #endregion
        #region PreviousHeistParticipants
        public List<HeistParticipant> PreviousHeistParticipants { get; set; } = new List<HeistParticipant>();
        #endregion
        #region HeistParticipants
        public List<HeistParticipant> HeistParticipants { get; set; } = new List<HeistParticipant>(); 
        #endregion
        #region HeistInProgress
        public bool HeistInProgress { get; set; } 
        #endregion
        #region TwitchChatClient
        public TwitchClient TwitchChatClient { get; set; }
        #endregion
        #region StreamElementsClient
        public StreamElementsClient StreamElementsClient { get; set; }
        #endregion

        #region Constructor
        public Heist(TwitchClient twitchChatClient, string channel)
        {
            TwitchChatClient = twitchChatClient;
            StreamElementsClient = new StreamElementsClient();
            HeistSettings = new HeistSettings();
            _channelName = channel;
        }
        #endregion

        #region StartHeist
        public void StartHeist(string startingUser)
        {
            HeistInProgress = true;
            LastHeistStart = DateTimeOffset.Now;
            TwitchChatClient.SendMessage(_channelName, HeistSettings.OnFirstEntryMessage.Replace("{user}", startingUser));
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                await Task.Delay(HeistSettings.StartDelay * 1000, _cts.Token);
                if (_cts.Token.IsCancellationRequested) return;
                await EndHeist();
            }, _cts.Token);
        }
        #endregion
        #region JoinHeist
        public async Task JoinHeist(string userName, bool isAll, int? points = null, bool resetUser = false)
        {
            var user = await StreamElementsClient.GetUser(userName);
            user.DisplayName = userName;
            if (!HeistInProgress && LastHeistEnd.HasValue &&
                LastHeistEnd.Value.AddSeconds(HeistSettings.Cooldown) > DateTimeOffset.Now)
            {
                TwitchChatClient.SendMessage(_channelName, $"{HeistSettings.WaitForCooldownMessage}: {Convert.ToInt32(HeistSettings.Cooldown - (DateTimeOffset.Now-LastHeistEnd.Value).TotalSeconds)} seconds remaining.");
            }
            if (LastHeistStart == null || !HeistInProgress && LastHeistEnd.HasValue && LastHeistEnd.Value.AddSeconds(HeistSettings.Cooldown) <= DateTimeOffset.Now)
            {
                if (!resetUser) if (await JoinAndSubtractPointsForUser(user, isAll, points)) StartHeist(userName);

            }
            else if (HeistInProgress)
            {
                await JoinAndSubtractPointsForUser(user, isAll, points, resetUser);
            }
        }
        #endregion
        #region EndHeist
        public async Task EndHeist(bool cancelHeist=false)
        {
            HeistInProgress = false;
            LastHeistEnd = DateTimeOffset.Now;

            if (cancelHeist)
            {
                try
                {
                    _cts.Cancel();
                    foreach (var participant in HeistParticipants)
                    {
                        await StreamElementsClient.AddOrRemovePointsFromUser(participant.User.Username, participant.Points);
                    }

                    TwitchChatClient.SendMessage(_channelName, HeistSettings.HeistCancelledMessage);
                    LastHeistEnd = DateTimeOffset.Now.AddSeconds(-300);
                }
                catch
                {
                    // Will only fail here if CancellationToken is already cancelled.. so whatever
                }
            }
            else
            {
                TwitchChatClient.SendMessage(_channelName, HeistSettings.OnSuccessfulStartMessage);
                var rnd = new Random();
                foreach (var participant in HeistParticipants)
                {
                    participant.WonHeist = rnd.Next(1, 100) < HeistSettings.ChanceToWinViewers;
                }

                if (HeistParticipants.All(a => a.WonHeist.HasValue && a.WonHeist.Value))
                {
                    TwitchChatClient.SendMessage(_channelName, HeistParticipants.Count > 1 ? HeistSettings.GroupOnAllWinMessage : HeistSettings.SoloOnWinMessage.Replace("{user}", HeistParticipants[0].User.DisplayName));
                }
                else if (HeistParticipants.All(a => a.WonHeist.HasValue && !a.WonHeist.Value))
                {
                    TwitchChatClient.SendMessage(_channelName, HeistParticipants.Count > 1 ? HeistSettings.GroupOnAllLoseMessage.Replace("{meatshields}", string.Join(',', HeistParticipants.Where(a => !a.WonHeist.Value).Select(a => $" riPepperonis {a.User.DisplayName}"))) : HeistSettings.SoloOnLossMessage.Replace("{user}", HeistParticipants[0].User.DisplayName));
                }
                else
                {
                    TwitchChatClient.SendMessage(_channelName, HeistSettings.GroupOnPartialWinMessage.Replace("{meatshields}", string.Join(',', HeistParticipants.Where(a => !a.WonHeist.Value).Select(a => $" riPepperonis {a.User.DisplayName}"))));
                }

                TwitchChatClient.SendMessage(_channelName, await DistributePointsAndGenerateResultString());
                if (HeistParticipants.Count(a => a.WonHeist is true) > 0 && HeistParticipants.Count(a => a.WonHeist is false) > 0)
                {
                    TwitchChatClient.SendMessage(_channelName, "This heist isn't over yet! Heist winners can !rez <UserName> for a chance to rez someone who did not make it out alive, sacrificing half of their winnings, but stopping the fallen from losing their bet. Failing to successful rez will result in a loss of winnings.");
                }
            }
            PreviousHeistParticipants = HeistParticipants.ToList();
            HeistParticipants = new List<HeistParticipant>();

        }
        #endregion
        #region RezUser
        public async Task RezUser(string rezzingUser, string rezzedUser)
        {
            if (HeistInProgress) TwitchChatClient.SendMessage(_channelName, $"Sorry {rezzingUser}, you cannot rez someone while a heist is still in progress!");
            var rezzingUserUser = PreviousHeistParticipants.FirstOrDefault(a => a.User.Username.ToLower() == rezzingUser.ToLower());
            if (rezzingUserUser == null)
            {
                TwitchChatClient.SendMessage(_channelName, $"Sorry {rezzingUser}, only people who participated in the last heist can rez!");
                return;
            }

            var rezzedUserUser = PreviousHeistParticipants.FirstOrDefault(a => a.User.Username.ToLower() == rezzedUser.ToLower());
            if (rezzedUserUser == null)
            {
                TwitchChatClient.SendMessage(_channelName, $"Sorry {rezzingUser}, you cannot rez someone who did not participate in the last heist!");
                return;
            }

            if (rezzingUserUser.WonHeist.Value == false)
            {
                TwitchChatClient.SendMessage(_channelName, $"Sorry {rezzingUser}, you cannot rez if you lost the last heist!");
                return;
            }

            if (rezzedUserUser.WonHeist.Value == true)
            {
                TwitchChatClient.SendMessage(_channelName, $"Sorry {rezzingUser}, you cannot rez someone who won the last heist!");
                return;
            }

            if (rezzedUserUser.WasRezzed.HasValue && rezzedUserUser.WasRezzed.Value == true)
            {
                TwitchChatClient.SendMessage(_channelName, $"Sorry {rezzingUser}, {rezzedUser} has already been rezzed.");
                return;
            }

            if (rezzingUserUser.UsedRez.HasValue && rezzingUserUser.UsedRez.Value == true)
            {
                TwitchChatClient.SendMessage(_channelName, $"Sorry {rezzingUser}, you can only rez one person per heist.");
                return;
            }

            if (rezzingUserUser.WonHeist.Value == true && rezzedUserUser.WonHeist.Value == false)
            {
                var rnd = new Random();
                if (rnd.Next(1, 100) < HeistSettings.ChanceToWinViewers)
                {
                    TwitchChatClient.SendMessage(_channelName, $"{rezzingUser} swooped in and sacrificed half of their heist winnings ({rezzingUserUser.Points / 2} to bring back {rezzedUser} from the dead and recover their original bet ({rezzedUserUser.Points})!");
                    await StreamElementsClient.AddOrRemovePointsFromUser(rezzingUserUser.User.Username, (rezzingUserUser.Points / 2) * -1);
                    await StreamElementsClient.AddOrRemovePointsFromUser(rezzedUserUser.User.Username, rezzedUserUser.Points);
                    rezzedUserUser.WasRezzed = true;
                }
                else
                {
                    TwitchChatClient.SendMessage(_channelName, $"{rezzingUser} got stunned while trying to rez {rezzedUser} and lost all there winnings({rezzingUserUser.Points / 2})!");
                    await StreamElementsClient.AddOrRemovePointsFromUser(rezzingUserUser.User.Username, (rezzingUserUser.Points) * -1);
                }
                rezzingUserUser.UsedRez = true;
            }
        }
        #endregion

        #region JoinAndSubtractPointsForUser
        private async Task<bool> JoinAndSubtractPointsForUser(StreamElementsUser user, bool isAll, int? points = null, bool resetUser = false)
        {
            if (resetUser)
            {
                if (HeistParticipants.Any(a => a.User.Username == user.Username))
                {
                    var me = HeistParticipants.FirstOrDefault(a => a.User.Username == user.Username);
                    if (me == null)
                    {
                        TwitchChatClient.SendMessage(_channelName, HeistSettings.HeistResetMeNotJoinedMessage.Replace("{user}", user.DisplayName));
                        return false;
                    }
                    TwitchChatClient.SendMessage(_channelName, HeistSettings.HeistResetMeMessage.Replace("{user}", user.DisplayName));
                    await StreamElementsClient.AddOrRemovePointsFromUser(me.User.Username, me.Points);
                    HeistParticipants.Remove(me);
                    if (!HeistParticipants.Any()) await this.EndHeist(true);
                }
                return false;
            }
            if (HeistParticipants.Any(a => a.User.Username == user.Username))
            {
                TwitchChatClient.SendMessage(_channelName, HeistSettings.UserAlreadyJoinedMessage.Replace("{user}", user.DisplayName));
                return false;
            }
            else
            {
                if (points.HasValue && points.Value > user.Points)
                {
                    TwitchChatClient.SendMessage(_channelName, HeistSettings.UserNotEnoughPointsMessage.Replace("{user}", user.DisplayName).Replace("{points}", user.Points.ToString()));
                    return false;
                }

                if (points.HasValue && points.Value > HeistSettings.MaxAmount)
                {
                    TwitchChatClient.SendMessage(_channelName, HeistSettings.UserOverMaxPointsMessage.Replace("{user}", user.DisplayName).Replace("{maxamount}", HeistSettings.MaxAmount.ToString()));
                    return false;
                }

                if (points.HasValue && points.Value < HeistSettings.MinEntries)
                {
                    TwitchChatClient.SendMessage(_channelName, HeistSettings.UserUnderMinPointsMessage.Replace("{user}", user.DisplayName).Replace("{minentries}", HeistSettings.MinEntries.ToString()));
                    return false;
                }

                var participant = new HeistParticipant { User = user };
                if (points.HasValue)
                {
                    await StreamElementsClient.AddOrRemovePointsFromUser(user.Username, -points.Value);
                    participant.Points = points.Value;
                }

                if (isAll)
                {
                    if (user.Points >= HeistSettings.MaxAmount)
                    {
                        await StreamElementsClient.AddOrRemovePointsFromUser(user.Username, -HeistSettings.MaxAmount);
                        participant.Points = HeistSettings.MaxAmount;
                    }
                    else
                    {
                        if (user.Points > 0)
                        {
                            await StreamElementsClient.AddOrRemovePointsFromUser(user.Username, -(int)user.Points);
                            participant.Points = (int)user.Points;
                        }
                        else
                        {
                            await StreamElementsClient.AddOrRemovePointsFromUser(user.Username, -1);
                            participant.Points = 1;
                        }
                    }
                }

                if (HeistParticipants.Count > 0)
                {
                    TwitchChatClient.SendMessage(_channelName, HeistSettings.OnEntryMessage.Replace("{user}", user.DisplayName));
                }

                HeistParticipants.Add(participant);
                return true;
            }
        } 
        #endregion
        #region DistributePointsAndGenerateResultString
        private async Task<string> DistributePointsAndGenerateResultString()
        {
            string resultString = "";
            foreach (var winner in HeistParticipants.Where(a => a.WonHeist.HasValue && a.WonHeist.Value))
            {
                await StreamElementsClient.AddOrRemovePointsFromUser(winner.User.Username, winner.Points * 2);
                if (string.IsNullOrWhiteSpace(resultString))
                {
                    resultString = $"Result: {winner.User.DisplayName} ({winner.Points * 2})";
                }
                else
                {
                    resultString = resultString + $", {winner.User.DisplayName} ({winner.Points * 2})";
                }
            }

            return resultString;
        } 
        #endregion
    }
}