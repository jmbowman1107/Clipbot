using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JeffBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bot = new JeffBot(new StreamerSettings
            {
                StreamerName = "",
                StreamerId = "",
                StreamerBotName = "",
                StreamerBotOauthToken = "",
                StreamElementsChannelId = "",
                StreamElementsJwtToken = "",
                BotFeatures = new List<BotFeatures>
                {
                    BotFeatures.BanHate,
                    BotFeatures.Heist,
                    BotFeatures.Clip,
                    BotFeatures.AdvancedClip,
                    BotFeatures.Mark
                }
            });
            //await bot.GetRecentFollowersAndBanHate();
            Console.ReadLine();
        }
    }
}