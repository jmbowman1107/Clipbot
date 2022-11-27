using Newtonsoft.Json;

namespace JeffBot
{
    public class StreamElementsUser
    {
        #region Channel
        [JsonProperty("channel")]
        public string Channel { get; set; }
        #endregion
        #region Username
        [JsonProperty("username")]
        public string Username { get; set; }
        #endregion
        #region Points
        [JsonProperty("points")]
        public long Points { get; set; }
        #endregion
        #region PointsAlltime
        [JsonProperty("pointsAlltime")]
        public long PointsAlltime { get; set; }
        #endregion
        #region Watchtime
        [JsonProperty("watchtime")]
        public long Watchtime { get; set; }
        #endregion
        #region Rank
        [JsonProperty("rank")]
        public long Rank { get; set; } 
        #endregion
        public string DisplayName { get; set; }
    }
}