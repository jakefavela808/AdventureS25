namespace AdventureS25;

using AdventureS25;

public static class ConversationCommandHandler
{
    private static Dictionary<string, Action<Command>> commandMap =
        new Dictionary<string, Action<Command>>()
        {
            {"y", Yes},
            {"n", No},
            {"leave", Leave},
            {"choose", ChooseStarter},
        };
    
    public static void Handle(Command command)
    {
        if (commandMap.ContainsKey(command.Verb))
        {
            Action<Command> action = commandMap[command.Verb];
            action.Invoke(command);
        }
    }

    private static string pendingStarterChoice = null;
    private static bool awaitingStarterSelection = false;

    private static void Yes(Command command)
    {
        if (pendingStarterChoice == "Nurse Noelia")
        {
            HealAllPals();
            pendingStarterChoice = null;
            States.ChangeState(StateTypes.Exploring);
        }
        else if (pendingStarterChoice == "Professor Jon")
        {
            PromptStarterSelection();
            awaitingStarterSelection = true;
        }
        else
        {
            Typewriter.TypeLine("You agreed");
        }
    }
    
    private static void No(Command command)
    {
        if (pendingStarterChoice != null)
        {
            Typewriter.TypeLine("You declined.");
            pendingStarterChoice = null;
            awaitingStarterSelection = false;
            States.ChangeState(StateTypes.Exploring);
        }
        else
        {
            Typewriter.TypeLine("You are disagreed");
        }
    }

    private static void Leave(Command command)
    {
        Typewriter.TypeLine("You leave the conversation.");
        pendingStarterChoice = null;
        awaitingStarterSelection = false;
        States.ChangeState(StateTypes.Exploring);
    }

    private static void Talk(Command command)
    {
        // Find the NPC at the current location
        var npcs = Player.CurrentLocation.GetNpcs();
        if (npcs.Count == 0)
        {
            Typewriter.TypeLine("There is no one here to talk to.");
            States.ChangeState(StateTypes.Exploring);
            return;
        }
        // For simplicity, talk to the first NPC present
        var npc = npcs[0];
        Typewriter.TypeLine($"You approach {npc.Name}.");
        Typewriter.TypeLine(npc.GetLocationDescription());
        if (npc.Name == "Nurse Noelia")
        {
            Typewriter.TypeLine("Would you like me to heal your Pals? (y/n)");
            pendingStarterChoice = "Nurse Noelia";
        }
        else if (npc.Name == "Professor Jon")
        {
            Typewriter.TypeLine("Would you like to choose your starter Pal? (y/n)");
            pendingStarterChoice = "Professor Jon";
        }
        else
        {
            Typewriter.TypeLine(npc.Description);
            States.ChangeState(StateTypes.Exploring);
        }
    }

    private static void ChooseStarter(Command command)
    {
        if (!awaitingStarterSelection)
        {
            Typewriter.TypeLine("No starter selection is pending.");
            return;
        }
        string choice = command.Noun.Trim().ToLower();
        string[] starters = { "sandie", "clyde capybara", "gloop glorp" };
        string selected = null;
        foreach (var s in starters)
        {
            if (choice == s.ToLower() || choice == s.Split(' ')[0])
            {
                selected = s;
                break;
            }
        }
        if (selected == null)
        {
            Typewriter.TypeLine("Invalid choice. Please type: choose sandie, choose clyde, or choose gloop.");
            return;
        }
        var pal = Pals.GetPalByName(selected);
        if (pal == null)
        {
            Typewriter.TypeLine("That Pal is not available.");
            return;
        }
        Player.AddPal(pal);
        Typewriter.TypeLine($"You chose {pal.Name} as your starter Pal!");
        awaitingStarterSelection = false;
        pendingStarterChoice = null;
        States.ChangeState(StateTypes.Exploring);
    }

    private static void HealAllPals()
    {
        if (Player.Pals.Count == 0)
        {
            Typewriter.TypeLine("You have no Pals to heal.");
        }
        else
        {
            foreach (var pal in Player.Pals)
            {
                pal.HP = pal.MaxHP;
            }
            Typewriter.TypeLine("All your Pals have been fully healed!");
        }
        pendingStarterChoice = null;
        States.ChangeState(StateTypes.Exploring);
    }

    private static void PromptStarterSelection()
    {
        Typewriter.TypeLine("Please choose your starter Pal: Sandie, Clyde Capybara, or Gloop Glorp.\nType: choose sandie, choose clyde, or choose gloop");
    }
}