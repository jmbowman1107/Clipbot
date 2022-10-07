using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams.CreateStreamMarker;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using OnLogArgs = TwitchLib.Client.Events.OnLogArgs;

namespace BanHateBot
{
    public class BanHateBot
    {
        public StreamerSettings StreamerSettings { get; set; }
        private TwitchClient _twitchChatClient;
        private TwitchPubSub _twitchPubSubClient;
        private TwitchAPI _twitchApi;
        private AdvancedClipper _advancedClipper;
        private Heist Heist { get; set; }

        #region Constructor
        public BanHateBot(StreamerSettings streamerSettings)
        {
            StreamerSettings = streamerSettings;
            InitializePubSub();
            InitializeChat();
            InitializeTwitchApi();
            _advancedClipper = new AdvancedClipper { StreamerSettings = StreamerSettings, TwitchApi = _twitchApi, TwitchChatClient = _twitchChatClient };
        } 
        #endregion

        #region GetRecentFollowersAndBanHate
        public async Task GetRecentFollowersAndBanHate()
        {
            GetUsersFollowsResponse followers;
            string pagniation = null;
            //do
            //{
            followers = await _twitchApi.Helix.Users.GetUsersFollowsAsync(first: 100, toId: StreamerSettings.StreamerId, after: pagniation);
            foreach (var follower in followers.Follows)
            {
                Console.WriteLine(follower.FromUserName);
                if (follower.FromUserName.Contains("hoss00312"))
                {
                    Console.WriteLine($"Banning this MOFO {follower.FromUserName}");
                    _twitchChatClient.BanUser(StreamerSettings.StreamerName.ToLower(), follower.FromUserName, "We don't tolerate hate in this channel. Goodbye.");
                }
            }
            pagniation = followers.Pagination.Cursor;
        }
        #endregion

        #region InitializePubSub
        private void InitializePubSub()
        {
            _twitchPubSubClient = new TwitchPubSub();
            _twitchPubSubClient.OnPubSubServiceConnected += PubSubClient_OnPubSubServiceConnected;
            _twitchPubSubClient.OnListenResponse += PubSubClient_OnListenResponse;
            _twitchPubSubClient.OnFollow += PubSubClient_OnFollow;
            _twitchPubSubClient.ListenToFollows(StreamerSettings.StreamerId);
            _twitchPubSubClient.Connect();
        }
        #endregion
        #region InitializeChat
        private void InitializeChat()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(StreamerSettings.StreamerBotName, StreamerSettings.StreamerBotOauthToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            _twitchChatClient = new TwitchClient(customClient);
            _twitchChatClient.Initialize(credentials, StreamerSettings.StreamerName.ToLower());

            _twitchChatClient.OnLog += ChatClient_OnLog;
            _twitchChatClient.OnJoinedChannel += ChatClient_OnJoinedChannel;
            _twitchChatClient.OnConnected += ChatClient_OnConnected;
            _twitchChatClient.OnMessageReceived += ChatClient_OnMessageReceived;
            _twitchChatClient.OnDisconnected += ChatClient_OnDisconnected;
            _twitchChatClient.Connect();
        }

        #endregion
        #region InitializeTwitchApi
        private void InitializeTwitchApi()
        {
            _twitchApi = new TwitchAPI();
            _twitchApi.Settings.ClientId = "";
            //_twitchApi.Settings.AccessToken = "";
            _twitchApi.Settings.AccessToken = "";
        }
        #endregion
        #region ChatClient_OnLog
        private void ChatClient_OnLog(object sender, OnLogArgs e)
        {
            //Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }
        #endregion
        #region ChatClient_OnDisconnected
        private void ChatClient_OnDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            // If we disconnect, wait 30 seconds, cleanup and reconnect.
            Task.Delay(30000).Wait();
            _twitchChatClient.OnLog -= ChatClient_OnLog;
            _twitchChatClient.OnJoinedChannel -= ChatClient_OnJoinedChannel;
            _twitchChatClient.OnConnected -= ChatClient_OnConnected;
            _twitchChatClient.OnMessageReceived -= ChatClient_OnMessageReceived;
            _twitchChatClient.OnDisconnected -= ChatClient_OnDisconnected;
            InitializeChat();
        }
        #endregion
        #region ChatClient_OnConnected
        private void ChatClient_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }
        #endregion
        #region ChatClient_OnJoinedChannel
        private void ChatClient_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
            _twitchChatClient.SendMessage(e.Channel, "Hey guys! I am and sitting and ready to ban all the hoss.");
            if (StreamerSettings.BotFeatures.Contains(BotFeatures.Heist)) 
            {
                Heist = new Heist(StreamerSettings, _twitchChatClient);
            }
        }
        #endregion
        #region ChatClient_OnMessageReceived
        private void ChatClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (StreamerSettings.BotFeatures.Contains(BotFeatures.BanHate))
            {
                if (e.ChatMessage.Username.Contains("hoss00312") || e.ChatMessage.Username.Contains("idwt_"))
                    _twitchChatClient.BanUser(e.ChatMessage.Channel, e.ChatMessage.Username, "We don't tolerate hate in this channel. Goodbye.");

                if (e.ChatMessage.Message.ToLower().Contains("buy followers"))
                {
                    var test = _twitchApi.Helix.Users.GetUsersFollowsAsync(fromId: e.ChatMessage.UserId, toId: StreamerSettings.StreamerId).Result;
                    if (test.Follows != null && !test.Follows.Any())
                    {
                        _twitchChatClient.BanUser(e.ChatMessage.Channel, e.ChatMessage.Username, "We don't want what you are selling.. go away.");
                    }
                }
            }

            if (StreamerSettings.BotFeatures.Contains(BotFeatures.Heist))
            {
                #region Heist Number
                var isHeistMessage = Regex.Match(e.ChatMessage.Message.ToLower(), @"^!heist \d+$");
                if (isHeistMessage.Captures.Count > 0)
                {
                    var number = Regex.Match(e.ChatMessage.Message, @"\d+$");
                    if (number.Captures.Count > 0)
                    {
                        Heist.JoinHeist(e.ChatMessage.DisplayName, false, Convert.ToInt32(number.Captures[0].Value)).Wait();
                    }
                }
                #endregion

                #region Heist All
                var isHeistAllMessage = Regex.Match(e.ChatMessage.Message.ToLower(), @"^!heist all$");
                if (isHeistAllMessage.Captures.Count > 0)
                {
                    Heist.JoinHeist(e.ChatMessage.DisplayName, true).Wait();
                }
                #endregion

                #region Heist Cancel
                var isHeistCancelMessage = Regex.Match(e.ChatMessage.Message.ToLower(), @"^!heist cancel$");
                if (isHeistCancelMessage.Captures.Count > 0)
                {
                    if (e.ChatMessage.IsBroadcaster || e.ChatMessage.IsModerator)
                    {
                        Heist.EndHeist(true).Wait();
                    }
                }
                #endregion

                #region Heist Reset Me
                var isHeistResetMeMessage = Regex.Match(e.ChatMessage.Message.ToLower(), @"^!heist undo$");
                if (isHeistResetMeMessage.Captures.Count > 0)
                {
                    Heist.JoinHeist(e.ChatMessage.DisplayName, false, null, true).Wait();
                }
                #endregion

                #region Heist Rez
                var isHeistRezMessage = Regex.Match(e.ChatMessage.Message.ToLower(), @"^!rez \S+$");
                if (isHeistRezMessage.Captures.Count > 0)
                {
                    var personToRez = e.ChatMessage.Message.Replace("!rez ", string.Empty);
                    if (personToRez.StartsWith("@"))
                    {
                        personToRez = personToRez.Remove(0, 1);
                    }
                    Heist.RezUser(e.ChatMessage.DisplayName, personToRez).Wait();
                }
                #endregion
            }

            if (StreamerSettings.BotFeatures.Contains(BotFeatures.Mark))
            {
                #region Mark
                var isMarkMessage = Regex.Match(e.ChatMessage.Message.ToLower(), @"^!mark$");
                if (isMarkMessage.Captures.Count > 0)
                {
                    MarkStream(e);
                }
                #endregion

                #region Mark Message
                var isMarkWithMessage = Regex.Match(e.ChatMessage.Message.ToLower(), @"^!mark .*$");
                if (isMarkWithMessage.Captures.Count > 0)
                {
                    var markDescription = Regex.Match(e.ChatMessage.Message.ToLower(), @" .*$");
                    if (markDescription.Captures.Count > 0)
                    {
                        MarkStream(e, markDescription.Captures[0].Value.Trim());
                    }
                }
                #endregion
            }

            if (StreamerSettings.BotFeatures.Contains(BotFeatures.Clip))
            {
                #region Clip
                var isClipMessage = Regex.Match(e.ChatMessage.Message.ToLower(), @"^!clip$");
                if (isClipMessage.Captures.Count > 0)
                {
                    _advancedClipper.CreateTwitchClip(e, StreamerSettings.BotFeatures.Contains(BotFeatures.AdvancedClip));
                }
                #endregion
            }

            if (StreamerSettings.BotFeatures.Contains(BotFeatures.AdvancedClip))
            {
                #region Clip Noobhunter
                var isPostNoobHunter = Regex.Match(e.ChatMessage.Message.ToLower(), @"^!clip noobhunter$");
                if (isPostNoobHunter.Captures.Count > 0)
                {
                    _advancedClipper.ValidateAndPostToNoobHuner(e);
                }
                #endregion
            }
        }
        #endregion

        #region PubSubClient_OnPubSubServiceConnected
        private void PubSubClient_OnPubSubServiceConnected(object sender, EventArgs e)
        {
            // SendTopics accepts an oauth optionally, which is necessary for some topics
            _twitchPubSubClient.SendTopics("oauth:ie4qryq4oeo45jflmg53dzpikmzqat");
        }
        #endregion
        #region PubSubClient_OnFollow
        private void PubSubClient_OnFollow(object sender, OnFollowArgs e)
        {
            if (StreamerSettings.BotFeatures.Contains(BotFeatures.BanHate))
            {
                if (e.Username.Contains("hoss00312") || e.Username.Contains("h0ss00312") || e.Username.Contains("moomoo4you") || e.Username.Contains("idwt_"))
                    _twitchChatClient.BanUser(StreamerSettings.StreamerName.ToLower(), e.Username, "We don't tolerate hate in this channel. Goodbye.");
            }
        }
        #endregion
        #region PubSubClient_OnListenResponse
        private void PubSubClient_OnListenResponse(object sender, OnListenResponseArgs e)
        {
            if (!e.Successful)
                throw new Exception($"Failed to listen! Response: {e.Response}");
        }
        #endregion

        #region MarkStream
        private void MarkStream(OnMessageReceivedArgs e, string markMessage = "Marked from bot.")
        {
            try
            {
                if (e.ChatMessage.IsVip || e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster)
                {
                    var mark = _twitchApi.Helix.Streams.CreateStreamMarkerAsync(new CreateStreamMarkerRequest { Description = markMessage, UserId = StreamerSettings.StreamerId }).Result;
                    if (markMessage != "Marked from bot.")
                    {
                        _twitchChatClient.SendMessage(e.ChatMessage.Channel, $"Stream successfully marked with description: \"{markMessage}\"");
                    }
                    else
                    {
                        _twitchChatClient.SendMessage(e.ChatMessage.Channel, "Stream successfully marked.");
                    }
                }
                else
                {
                    _twitchChatClient.SendMessage(e.ChatMessage.Channel, $"Sorry {e.ChatMessage.Username}, only {e.ChatMessage.Channel}, VIPS, and Moderators can mark the stream.");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Source == "Newtonsoft.Json")
                {
                    if (markMessage != "Marked from bot.")
                    {
                        _twitchChatClient.SendMessage(e.ChatMessage.Channel, $"Stream successfully marked with description: \"{markMessage}\"");
                    }
                    else
                    {
                        _twitchChatClient.SendMessage(e.ChatMessage.Channel, "Stream successfully marked.");
                    }
                }
                else
                {
                    _twitchChatClient.SendMessage(e.ChatMessage.Channel,  "Stream was NOT successfully marked.. Someone tell Jeff..");
                }
            }
        }
        #endregion
    }
}
