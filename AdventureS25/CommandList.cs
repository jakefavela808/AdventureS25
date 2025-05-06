namespace AdventureS25;

using AdventureS25;

public static class CommandList
{
    public static readonly string[] explorationCommands = 
    {
        "go [direction]", "take [item]", "drop [item]", "look", 
        "inventory", "talk", "fight", "pals", "eat [item]", "drink [item]", 
        "read [item]", "open chest", "mute"
    };

    public static String exploreCommands = "\n============ AVAILABLE COMMANDS ============\ngo [direction] - Move in a direction (north, south, east, west)\ntake [item] - Pick up an item\nread [item] - Read an item\ntalk - Talk to a character\nfight - Fight a wild pal\ninventory - Check your inventory\npals - Check your Pals\nopen chest - Open a chest\nmute - Toggle game audio on/off\n============================================\n";
    public static String conversationCommands = "\n============ AVAILABLE COMMANDS ============\nyes - Accept an offer or respond positively\nno - Decline an offer or respond negatively\nleave - Leave the conversation\n[number] - Select a numbered dialogue option\n============================================\n";
    public static String combatCommands = "\n============ BATTLE COMMANDS ============\nbasic - Basic Attack\nspecial - Special Attack\ndefend - Defend\npotion - Restore some of your Pals HP\ntame - Tame the wild pal\nrun - Run\n=========================================\n";
}