namespace AdventureS25;

using AdventureS25;
using System.Linq;

public static class ExplorationCommandHandler
{
    private static Dictionary<string, Action<Command>> commandMap =
        new Dictionary<string, Action<Command>>()
        {
            {"eat", Eat},
            {"go", Move},
            {"tron", Tron},
            {"troff", Troff},
            {"take", Take},
            {"inventory", ShowInventory},
            {"look", Look},
            {"drop", Drop},
            {"nouns", Nouns},
            {"verbs", Verbs},
            {"fight", ChangeToFightState},
            {"explore", ChangeToExploreState},
            {"talk", ChangeToTalkState},
            {"drink", Drink},
            {"beerme", SpawnBeerInInventory},
            {"unbeerme", UnSpawnBeerInInventory},
            {"puke", Puke},
            {"tidyup", TidyUp},
            {"teleport", Teleport},
            {"connect", Connect},
            {"disconnect", Disconnect},
            {"read", Read},
            {"pals", ShowPals}
        };
    private static void Read(Command obj)
    {
        Player.Read(obj);
    }
    private static void Disconnect(Command obj)
    {
        Conditions.ChangeCondition(ConditionTypes.IsRemovedConnection, true);
    }

    private static void Connect(Command obj)
    {
        Conditions.ChangeCondition(ConditionTypes.IsCreatedConnection, true);
    }

    private static void Teleport(Command obj)
    {
        Conditions.ChangeCondition(ConditionTypes.IsTeleported, true);
    }   

    private static void TidyUp(Command command)
    {
        Conditions.ChangeCondition(ConditionTypes.IsTidiedUp, true);
    }

    private static void Puke(Command obj)
    {
        Conditions.ChangeCondition(ConditionTypes.IsHungover, true);
    }

    private static void UnSpawnBeerInInventory(Command command)
    {
        Conditions.ChangeCondition(ConditionTypes.IsBeerMed, false);

    }

    private static void SpawnBeerInInventory(Command command)
    {
        Conditions.ChangeCondition(ConditionTypes.IsBeerMed, true);
    }

    private static void Drink(Command command)
    {
        Player.Drink(command);
    }

    private static void ChangeToTalkState(Command command)
    {
        // Block talking to Professor Jon after starter received
        var npcs = Player.CurrentLocation.GetNpcs();
        if (npcs.Any(npc => npc.Name == "Professor Jon") && Conditions.IsTrue(ConditionTypes.HasReceivedStarter))
        {
            Typewriter.TypeLine("You have already received your first Pal. Professor Jon is busy right now and doesn't have anything else for you.");
            Console.Clear();
            Player.Look();
            return;
        }
        ConversationCommandHandler.Talk(command);
    }
    
    private static void ChangeToFightState(Command obj)
    {
        CombatCommandHandler.Fight(obj);
    }
    
    private static void ChangeToExploreState(Command obj)
    {
        States.ChangeState(StateTypes.Exploring);
    }

    private static void Verbs(Command command)
    {
        List<string> verbs = ExplorationCommandValidator.GetVerbs();
        foreach (string verb in verbs)
        {
            Typewriter.TypeLine(verb);
        }
    }

    private static void Nouns(Command command)
    {
        List<string> nouns = ExplorationCommandValidator.GetNouns();
        foreach (string noun in nouns)
        {
            Typewriter.TypeLine(noun);
        }
    }

    public static void Handle(Command command)
    {
        if (commandMap.ContainsKey(command.Verb))
        {
            Action<Command> method = commandMap[command.Verb];
            method.Invoke(command);
        }
        else
        {
            Typewriter.TypeLine("I don't know how to do that.");
            Console.Clear();
            Player.Look();
        }
    }
    
    private static void Drop(Command command)
    {
        Player.Drop(command);
    }
    
    private static void Look(Command command)
    {
        Player.Look();
    }

    private static void ShowInventory(Command command)
    {
        Player.ShowInventory();
    }

    private static void ShowPals(Command command)
    {
        Player.ShowPals();
    }
    
    private static void Take(Command command)
    {
        Player.Take(command);
    }

    private static void Troff(Command command)
    {
        Debugger.Troff();
    }

    private static void Tron(Command command)
    {
        Debugger.Tron();
    }

    public static void Eat(Command command)
    {
        Typewriter.TypeLine("Eating..." + command.Noun);
    }

    public static void Move(Command command)
    {
        Player.Move(command);
    }
}