namespace BanHateBot
{
    public class HeistSettings
    {
        #region MinEntries
        public int MinEntries { get; set; } = 1;
        #endregion
        #region MaxAmount
        public int MaxAmount { get; set; } = 25252;
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
        public string OnFailedStartMessage { get; set; } = "Oof. {user} was spotted by a parasitic wasp and had to take a detour. No pollinating for {user.name} for now. Better luck next time!";
        #endregion
        #region SoloOnWinMessage
        public string SoloOnWinMessage { get; set; } = "WOOP! {user} made it back from the peonies garden and managed to successfully pollinate the garden!";
        #endregion
        #region SoloOnLossMessage
        public string SoloOnLossMessage { get; set; } = "Oh no! {user} was chased by an owl and lost all of the pollens carried. {user} did make it out alive and back to porchlight.";
        #endregion
        #region GroupOnAllWinMessage
        public string GroupOnAllWinMessage { get; set; } = "BOO-YAH! The entire Eclipse crew successfully pollinated the peonies garden. They made it safely back to the porch, and they celebrate by fluttering together under the bright moonlight.";
        #endregion
        #region GroupOnPartialWinMessage
        public string GroupOnPartialWinMessage { get; set; } = "A nest of hornets attacked some of the eclipse crew. The rest of the crew was able to escape. Let's thank our meat shields for saving us: RIP {meatshields}";
        #endregion
        #region GroupOnAllLoseMessage
        public string GroupOnAllLoseMessage { get; set; } = "A giant owl had emerged from the darkness and devoured the entire eclipse crew. No one made it back home into the light. HIKS. {meatshields}";
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
        #region HeistResetMeNotJoinedMessage
        public string HeistResetMeNotJoinedMessage { get; set; } = "vlycaPain {user} was never part of the heist, but pretending that they were.. Kappa";
        #endregion
        #region HeistResetMeMessage
        public string HeistResetMeMessage { get; set; } = "vlycaPain {user} has left the heist.. Let's hope they just needed to take a quick bio break..";
        #endregion
        #region HeistCancelledMessage
        public string HeistCancelledMessage { get; set; } = "Heist has been cancelled, all points have been returned";
        #endregion
    }
}