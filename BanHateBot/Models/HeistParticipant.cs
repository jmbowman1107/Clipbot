namespace BanHateBot.Models
{
    public class HeistParticipant
    {
        #region User
        public StreamElementsUser User { get; set; }
        #endregion
        #region Points
        public int Points { get; set; }
        #endregion
        #region WonHeist
        public bool? WonHeist { get; set; }
        #endregion
        #region WasRezzed
        public bool? WasRezzed { get; set; } 
        #endregion
    }
}