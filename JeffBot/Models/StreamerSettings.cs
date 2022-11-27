using System.Collections.Generic;

namespace JeffBot
{
    public class StreamerSettings
    {
        #region StreamerName
        public string StreamerName { get; set; }
        #endregion
        #region StreamerId
        public string StreamerId { get; set; }
        #endregion
        #region StreamerBotName
        public string StreamerBotName { get; set; }
        #endregion
        #region StreamerBotOauthToken
        public string StreamerBotOauthToken { get; set; }
        #endregion
        #region StreamElementsChannelId
        public string StreamElementsChannelId { get; set; }
        #endregion
        #region StreamElementsJwtToken
        public string StreamElementsJwtToken { get; set; }
        #endregion
        #region BotFeatures
        public List<BotFeatures> BotFeatures { get; set; } 
        #endregion
    }

    public enum BotFeatures
    {
        BanHate,
        Heist,
        Clip,
        AdvancedClip,
        Mark,
    }
}
