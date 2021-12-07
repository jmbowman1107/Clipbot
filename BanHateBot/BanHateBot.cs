using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users.GetUserFollows;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using OnLogArgs = TwitchLib.Client.Events.OnLogArgs;

namespace BanHateBot
{
    public class BanHateBot
    {
        private TwitchClient _twitchChatClient;
        private TwitchPubSub _twitchPubSubClient;
        private TwitchAPI _twitchApi;
        private Heist Heist { get; set; }

        #region Constructor
        public BanHateBot()
        {
            InitializePubSub();
            InitializeChat();
            InitializeTwitchApi();
        } 
        #endregion

        #region GetRecentFollowersAndBanHate
        public async Task GetRecentFollowersAndBanHate()
        {
            GetUsersFollowsResponse followers;
            string pagniation = null;
            //do
            //{
            followers = await _twitchApi.Helix.Users.GetUsersFollowsAsync(first: 100, toId: "75230612", after: pagniation);
            foreach (var follower in followers.Follows)
            {
                Console.WriteLine(follower.FromUserName);
                if (follower.FromUserName.Contains("hoss00312"))
                {
                    Console.WriteLine($"Banning this MOFO {follower.FromUserName}");
                    _twitchChatClient.BanUser("vlyca", follower.FromUserName, "We don't tolerate hate in this channel. Goodbye.");
                }
            }
            pagniation = followers.Pagination.Cursor;
            //} while (followers.Pagination.Cursor != null);

        }
        #endregion

        #region InitializePubSub
        private void InitializePubSub()
        {
            _twitchPubSubClient = new TwitchPubSub();
            _twitchPubSubClient.OnPubSubServiceConnected += PubSubClient_OnPubSubServiceConnected;
            _twitchPubSubClient.OnListenResponse += PubSubClient_OnListenResponse;
            _twitchPubSubClient.OnFollow += PubSubClient_OnFollow;
            _twitchPubSubClient.ListenToFollows("75230612");
            _twitchPubSubClient.Connect();
        }
        #endregion
        #region InitializeChat
        private void InitializeChat()
        {
            ConnectionCredentials credentials =
                new ConnectionCredentials("moonameisjeff", "oauth:ie4qryq4oeo45jflmg53dzpikmzqat");
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            _twitchChatClient = new TwitchClient(customClient);
            _twitchChatClient.Initialize(credentials, "vlyca");

            _twitchChatClient.OnLog += ChatClient_OnLog;
            _twitchChatClient.OnJoinedChannel += ChatClient_OnJoinedChannel;
            _twitchChatClient.OnConnected += ChatClient_OnConnected;
            _twitchChatClient.OnMessageReceived += ChatClient_OnMessageReceived;
            _twitchChatClient.Connect();
        }
        #endregion
        #region InitializeTwitchApi
        private void InitializeTwitchApi()
        {
            _twitchApi = new TwitchAPI();
            _twitchApi.Settings.ClientId = "5j1aae4x7qqx17shppz7tc2g9rd6fw";
            _twitchApi.Settings.Secret = "ubx843ckzgxlzwt1558wlicsf6yuir";
        }
        #endregion

        #region ChatClient_OnLog
        private void ChatClient_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
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
            //_twitchChatClient.SendMessage(e.Channel, "Hey guys! I am and sitting and ready to ban all the hoss.");
            Heist = new Heist(_twitchChatClient, e.Channel);
        }
        #endregion
        #region ChatClient_OnMessageReceived
        private void ChatClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Username.Contains("hoss00312") || e.ChatMessage.Username.Contains("moomoo4you") || e.ChatMessage.Username.Contains("idwt_"))
                _twitchChatClient.BanUser(e.ChatMessage.Channel, e.ChatMessage.Username, "We don't tolerate hate in this channel. Goodbye.");

            var isHeistMessage = Regex.Match(e.ChatMessage.Message, @"^!heist \d+$");
            if (isHeistMessage.Captures.Count > 0)
            {
                var number = Regex.Match(e.ChatMessage.Message, @"\d+$");
                if (number.Captures.Count > 0)
                {
                    Heist.JoinHeist("MooNameIsJeff", false, Convert.ToInt32(number.Captures[0].Value)).Wait();
                }
            }
            var isHeistAllMessage = Regex.Match(e.ChatMessage.Message, @"^!heist all$");
            if (isHeistAllMessage.Captures.Count > 0)
            {
                Heist.JoinHeist("MooNameIsJeff", true).Wait();
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
            if (e.Username.Contains("hoss00312") || e.Username.Contains("h0ss00312") || e.Username.Contains("moomoo4you") || e.Username.Contains("idwt_"))
                _twitchChatClient.BanUser("vlyca", e.Username, "We don't tolerate hate in this channel. Goodbye.");
        }
        #endregion
        #region PubSubClient_OnListenResponse
        private void PubSubClient_OnListenResponse(object sender, OnListenResponseArgs e)
        {
            if (!e.Successful)
                throw new Exception($"Failed to listen! Response: {e.Response}");
        }
        #endregion
    }
}
