namespace AdventureS25;

public enum ConditionTypes
{
    HasReadNote,
    HasReceivedStarter,
    HasDefeatedFirstPal, // Added for tutorial sequence
    HasKey,  
    IsTiny,   
    IsDrunk,  
    IsBeerMed,
    IsHungover,
    IsTidiedUp,
    IsTeleported,
    IsCreatedConnection,
    IsRemovedConnection,
    PlayerNeedsFirstHealFromNoelia, // Triggers Noelia's initial quest dialogue
    PlayerHasPotionForMatt,           // Player has the potion to deliver to Matt
    MattHasRevealedCave            // Matt has told the player about the secret cave
}