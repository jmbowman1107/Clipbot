using System;
using System.Collections.Generic;
using System.Linq;
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
        #endregion

        #region HeistSettings
        public HeistSettings HeistSettings { get; set; } 
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
            Task.Run(async () =>
            {
                await Task.Delay(HeistSettings.StartDelay * 1000);
                await EndHeist();
            });
        }
        #endregion
        #region JoinHeist
        public async Task JoinHeist(string userName, bool isAll, int? points = null)
        {
            var user = await StreamElementsClient.GetUser(userName);
            if (!HeistInProgress && LastHeistEnd.HasValue &&
                LastHeistEnd.Value.AddSeconds(HeistSettings.Cooldown) > DateTimeOffset.Now)
            {
                TwitchChatClient.SendMessage(_channelName, HeistSettings.WaitForCooldownMessage);
            }
            if (LastHeistStart == null || !HeistInProgress && LastHeistEnd.HasValue && LastHeistEnd.Value.AddSeconds(HeistSettings.Cooldown) <= DateTimeOffset.Now)
            {
                if (await JoinAndSubtractPointsForUser(user, isAll, points)) StartHeist(userName);
            }
            else if (HeistInProgress)
            {
                await JoinAndSubtractPointsForUser(user, isAll, points);
            }
        }
        #endregion
        #region EndHeist
        public async Task EndHeist()
        {
            HeistInProgress = false;
            LastHeistEnd = DateTimeOffset.Now;

            TwitchChatClient.SendMessage(_channelName, HeistSettings.OnSuccessfulStartMessage);

            var rnd = new Random();
            foreach (var participant in HeistParticipants)
            {
                participant.WonHeist = rnd.Next(1, 100) <  HeistSettings.ChanceToWinViewers;
            }

            if (HeistParticipants.All(a => a.WonHeist.HasValue && a.WonHeist.Value))
            {
                TwitchChatClient.SendMessage(_channelName, HeistParticipants.Count > 1 ? HeistSettings.GroupOnAllWinMessage : HeistSettings.SoloOnWinMessage);
            }
            else if (HeistParticipants.All(a => a.WonHeist.HasValue && !a.WonHeist.Value))
            {
                TwitchChatClient.SendMessage(_channelName, HeistParticipants.Count > 1 ? HeistSettings.GroupOnAllLoseMessage : HeistSettings.SoloOnLossMessage);
            }
            else
            {
                TwitchChatClient.SendMessage(_channelName, HeistSettings.GroupOnPartialWinMessage);
            }

            TwitchChatClient.SendMessage(_channelName, await GenerateResultString());
            HeistParticipants = new List<HeistParticipant>();

        }
        #endregion

        #region JoinAndSubtractPointsForUser
        private async Task<bool> JoinAndSubtractPointsForUser(StreamElementsUser user, bool isAll, int? points = null)
        {
            if (HeistParticipants.Any(a => a.User.Username == user.Username))
            {
                TwitchChatClient.SendMessage(_channelName, HeistSettings.UserAlreadyJoinedMessage.Replace("{user}", user.Username));
                return false;
            }
            else
            {
                if (points.HasValue && points.Value > user.Points)
                {
                    TwitchChatClient.SendMessage(_channelName, HeistSettings.UserNotEnoughPointsMessage.Replace("{user}", user.Username).Replace("{points}", user.Points.ToString()));
                    return false;
                }

                if (points.HasValue && points.Value > HeistSettings.MaxAmount)
                {
                    TwitchChatClient.SendMessage(_channelName, HeistSettings.UserOverMaxPointsMessage.Replace("{user}", user.Username).Replace("{maxamount}", HeistSettings.MaxAmount.ToString()));
                    return false;
                }

                if (points.HasValue && points.Value < HeistSettings.MinEntries)
                {
                    TwitchChatClient.SendMessage(_channelName, HeistSettings.UserUnderMinPointsMessage.Replace("{user}", user.Username).Replace("{minentries}", HeistSettings.MinEntries.ToString()));
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
                    TwitchChatClient.SendMessage(_channelName, HeistSettings.OnEntryMessage.Replace("{user}", user.Username));
                }

                HeistParticipants.Add(participant);
                return true;
            }
        } 
        #endregion
        #region GenerateResultString
        private async Task<string> GenerateResultString()
        {
            string resultString = "";
            foreach (var winner in HeistParticipants.Where(a => a.WonHeist.HasValue && a.WonHeist.Value))
            {
                await StreamElementsClient.AddOrRemovePointsFromUser(winner.User.Username, winner.Points * 2);
                if (string.IsNullOrWhiteSpace(resultString))
                {
                    resultString = $"Result: {winner.User.Username} ({winner.Points * 2})";
                }
                else
                {
                    resultString = resultString + $", {winner.User.Username} ({winner.Points * 2})";
                }
            }

            return resultString;
        } 
        #endregion
    }
}