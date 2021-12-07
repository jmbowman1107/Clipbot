namespace BanHateBot
{
    public class HeistSettings
    {
        #region MinEntries
        public int MinEntries { get; set; } = 1;
        #endregion
        #region MaxAmount
        public int MaxAmount { get; set; } = 1337;
        #endregion
        #region StartDelay
        public int StartDelay { get; set; } = 120;
        #endregion
        #region Cooldown
        public int Cooldown { get; set; } = 300;
        #endregion
        #region ChanceToWinViewers
        public int ChanceToWinViewers { get; set; } = 75;
        #endregion
        #region WinnerPayoutViewers
        public int WinnerPayoutViewers { get; set; } = 100;
        #endregion
        #region OnFirstEntryMessage
        public string OnFirstEntryMessage { get; set; } = "Eclipse! {user} is trying to get a crew together for a round of pollinating the peonies garden. Type !heist <amount> to join the crew.";
        #endregion
        #region OnEntryMessage
        public string OnEntryMessage { get; set; } = "Ahoy! @{user} has joined the treasure hunt.";
        #endregion
        #region OnSuccessfulStartMessage
        public string OnSuccessfulStartMessage { get; set; } = "The crew fluff up their fluff and shakes their wings, getting ready to leave the comfort of the porchlight and fly to the peonies garden.";
        #endregion
        #region OnFailedStartMessage
        public string OnFailedStartMessage { get; set; }
        #endregion
        #region SoloOnWinMessage
        public string SoloOnWinMessage { get; set; } = "Solo Win Message. Congrats!";
        #endregion
        #region SoloOnLossMessage
        public string SoloOnLossMessage { get; set; } = "Solo Loss Message";
        #endregion
        #region GroupOnAllWinMessage
        public string GroupOnAllWinMessage { get; set; } = "BOO-YAH! The entire Eclipse crew successfully pollinated the peonies garden. They made it safely back to the porch, and they celebrate by fluttering together under the bright moonlight.";
        #endregion
        #region GroupOnPartialWinMessage
        public string GroupOnPartialWinMessage { get; set; } = "A nest of hornets attacked some of the eclipse crew. The rest of the crew was able to escape, thankfully!";
        #endregion
        #region GroupOnAllLoseMessage
        public string GroupOnAllLoseMessage { get; set; } = "Group All Lose Message";
        #endregion
        #region WaitForCooldownMessage
        public string WaitForCooldownMessage { get; set; } = "Hey hey hey, cool it, if we heist too often, there is no way we can succeed. Please wait a try to start a heist again a little bit later.";
        #endregion
        #region UserAlreadyJoinedMessage
        public string UserAlreadyJoinedMessage { get; set; } = "{user}, you have already joined this heist.";
        #endregion
        #region UserNotEnoughPointsMessage
        public string UserNotEnoughPointsMessage { get; set; } = "{user}, you only have {points} points to heist with, try to enter again.";
        #endregion
        #region UserOverMaxPointsMessage
        public string UserOverMaxPointsMessage { get; set; } = "{user}, you can only heist up to {maxamount} points, try to enter again.";
        #endregion
        #region UserUnderMinPointsMessage
        public string UserUnderMinPointsMessage { get; set; } = "{user}, you must heist with at least {minentries} points, try to enter again."; 
        #endregion
    }
}