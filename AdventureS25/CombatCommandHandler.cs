namespace AdventureS25;

public static class CombatCommandHandler
{
    private static Dictionary<string, Action<Command>> commandMap =
        new Dictionary<string, Action<Command>>()
        {
            {"basic", BasicAttack},
            {"special", SpecialAttack},
            {"defend", Defend},
            {"potion", Potion},
            {"tame", Tame},
            {"run", Run},
        };

    public static void Fight(Command command)
    {
        // Get the wild pal at the current location
        var wildPal = Player.CurrentLocation.pals?.Count > 0 ? Player.CurrentLocation.pals[0] : null;
        if (wildPal == null)
        {
            Typewriter.TypeLine("There is no wild Pal here to fight!");
            Console.Clear();
            Player.Look();
            return;
        }
        // Use the first Pal in the player's inventory for battle
        if (Player.Pals == null || Player.Pals.Count == 0)
        {
            Typewriter.TypeLine("You don't have a Pal to fight with!");
            Console.Clear();
            Player.Look();
            return;
        }
        var playerPal = Player.Pals[0];
        BattleManager.StartBattle(playerPal, wildPal);
        States.ChangeState(StateTypes.Fighting);
        Console.Clear();
        States.ChangeState(StateTypes.Fighting);
        // Print full battle UI/UX
        Console.WriteLine("================ BATTLE START ================");
        Typewriter.TypeLine($"You send out: {playerPal.Name}!");
        Console.WriteLine(GetAsciiArt(playerPal.AsciiArt));
        Typewriter.TypeLine($"{playerPal.Description}");
        Typewriter.TypeLine($"HP: {playerPal.HP}/{playerPal.MaxHP}");
        if (playerPal.Moves != null && playerPal.Moves.Count > 0)
        {
            Typewriter.TypeLine($"Moves: {string.Join(", ", playerPal.Moves)}");
        }
        Typewriter.TypeLine("");
        Typewriter.TypeLine($"A wild {wildPal.Name} appears!");
        Console.WriteLine(GetAsciiArt(wildPal.AsciiArt));
        Typewriter.TypeLine($"{wildPal.Description}");
        Typewriter.TypeLine($"HP: {wildPal.HP}/{wildPal.MaxHP}");
        if (wildPal.Moves != null && wildPal.Moves.Count > 0)
        {
            Typewriter.TypeLine($"Moves: {string.Join(", ", wildPal.Moves)}");
        }
        Console.WriteLine("==============================================");
    }
    
    public static void Handle(Command command)
    {
        if (commandMap.ContainsKey(command.Verb))
        {
            Action<Command> action = commandMap[command.Verb];
            action.Invoke(command);
        }
    }

    private static void BasicAttack(Command command)
    {
        if (BattleManager.IsBattleActive)
            BattleManager.HandlePlayerAction("basic");
        else
            Typewriter.TypeLine("You attack it in the face parts");
    }
    
    private static void SpecialAttack(Command command)
    {
        if (BattleManager.IsBattleActive)
            BattleManager.HandlePlayerAction("special");
        else
            Typewriter.TypeLine("You attack it in the face parts");
    }
    
    private static void Tame(Command command)
    {
        if (BattleManager.IsBattleActive)
            BattleManager.HandlePlayerAction("tame");
        else
            Typewriter.TypeLine("You try to tame it");
    }
    
    private static void Defend(Command command)
    {
        if (BattleManager.IsBattleActive)
            BattleManager.HandlePlayerAction("defend");
        else
            Typewriter.TypeLine("You defend it in the face parts");
    }

    private static void Potion(Command command)
    {
        if (BattleManager.IsBattleActive)
            BattleManager.HandlePlayerAction("potion");
        else
            Typewriter.TypeLine("You quaff the potion parts");
    }
    
    private static void Run(Command command)
    {   
        if (BattleManager.IsBattleActive)
            BattleManager.HandlePlayerAction("run");
        else
        {
            Typewriter.TypeLine("You flee");
            States.ChangeState(StateTypes.Exploring);
        }
    }

    // Helper method to resolve AsciiArt keys to actual art
    private static string GetAsciiArt(string artKey)
    {
        if (string.IsNullOrEmpty(artKey)) return "";
        if (!artKey.StartsWith("AsciiArt.")) return artKey;
        var type = typeof(AsciiArt);
        var fieldName = artKey.Substring("AsciiArt.".Length);
        var field = type.GetField(fieldName);
        if (field != null)
        {
            return field.GetValue(null)?.ToString() ?? artKey;
        }
        var propInfo = type.GetProperty(fieldName);
        if (propInfo != null)
        {
            return propInfo.GetValue(null)?.ToString() ?? artKey;
        }
        return artKey;
    }
}