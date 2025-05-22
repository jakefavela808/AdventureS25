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
        Pal? wildPal = Player.CurrentLocation?.ActiveWildPal;

        if (wildPal == null)
        {
            Typewriter.TypeLine("There's no wild Pal here to fight.");
            Console.Clear();
            Player.Look();
            return;
        }

        if (!Player.Pals.Any())
        {
            Typewriter.TypeLine("You have no Pals to fight with!");
            return;
        }
        Typewriter.TypeLine($"A wild {wildPal.Name} appears!");
        BattleManager.StartBattle(null, wildPal);
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
        if (BattleManager.PlayerPal != null && BattleManager.PlayerPal.BasicAttackUses > 0)
        {
            if (BattleManager.IsBattleActive)
            {
                BattleManager.PlayerPal.BasicAttackUses--;
                BattleManager.HandlePlayerAction("basic");
            }
            else
            {
                Typewriter.TypeLine("You can only use basic attacks in a battle.");
            }
        }
        else if (BattleManager.PlayerPal == null)
        {
            Typewriter.TypeLine("No active Pal to perform basic attack!");
        }
        else
        {
            Typewriter.TypeLine("Your Pal is out of energy for basic attacks!");
        }
    }
    
    private static void SpecialAttack(Command command)
    {
        if (BattleManager.PlayerPal != null && BattleManager.PlayerPal.SpecialAttackUses > 0)
        {
            if (BattleManager.IsBattleActive)
            {
                BattleManager.PlayerPal.SpecialAttackUses--;
                BattleManager.HandlePlayerAction("special");
            }
            else
            {
                Typewriter.TypeLine("You can only use special attacks in a battle.");
            }
        }
        else if (BattleManager.PlayerPal == null)
        {
            Typewriter.TypeLine("No active Pal to perform special attack!");
        }
        else
        {
            Typewriter.TypeLine("Your Pal is out of energy for special attacks!");
        }
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
    public static string GetAsciiArt(string artKey)
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