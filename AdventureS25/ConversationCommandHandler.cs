namespace AdventureS25;

using AdventureS25;

public static class ConversationCommandHandler
{
    private static Dictionary<string, Action<Command>> commandMap =
        new Dictionary<string, Action<Command>>()
        {
            {"yes", Yes},
            {"no", No},
            {"leave", Leave},
            {"1", ChooseStarter},
            {"2", ChooseStarter},
            {"3", ChooseStarter},
        };
    
    public static void Handle(Command command)
    {
        if (commandMap.ContainsKey(command.Verb))
        {
            Action<Command> action = commandMap[command.Verb];
            action.Invoke(command);
        }
        // Remove any legacy 'choose' verb handling
        // No need to parse 'choose' as a command anymore
    }

    private static string pendingStarterChoice = null;
    private static bool awaitingStarterSelection = false;

    public static bool IsAwaitingStarterSelection()
    {
        return awaitingStarterSelection;
    }

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
            if (Conditions.IsTrue(ConditionTypes.HasReceivedStarter))
            {
                Typewriter.TypeLine("Professor Jon is busy right now and doesn't have anything else for you.");
                pendingStarterChoice = null;
                awaitingStarterSelection = false;
                States.ChangeState(StateTypes.Exploring);
                return;
            }
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
            Console.Clear();
            Player.Look();
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

    public static void Talk(Command command)
    {
        // Find the NPC at the current location
        var npcs = Player.CurrentLocation.GetNpcs();
        if (npcs.Count == 0)
        {
            Typewriter.TypeLine("There is no one here to talk to.");
            States.ChangeState(StateTypes.Exploring);
            return;
        }
        // Talk to the first NPC
        Console.Clear();
        States.ChangeState(StateTypes.Talking);
        // Console.WriteLine(CommandList.conversationCommands); // Moved to after dialogue

        var npc = npcs[0];
        // Approach and display art
        Typewriter.TypeLine($"You approach {npc.Name}.");
        string art = npc.AsciiArt;
        if (!string.IsNullOrEmpty(art) && art.StartsWith("AsciiArt."))
        {
            var type = typeof(AsciiArt);
            var fieldName = art.Substring("AsciiArt.".Length);
            var field = type.GetField(fieldName);
            if (field != null)
                art = field.GetValue(null)?.ToString() ?? art;
            else
            {
                var propInfo = type.GetProperty(fieldName);
                if (propInfo != null)
                    art = propInfo.GetValue(null)?.ToString() ?? art;
            }
        }
        if (!string.IsNullOrEmpty(art))
            Console.WriteLine(art);
        // Initial description
        Typewriter.TypeLine(npc.Description);

        // Professor Jon logic
        if (npc.Name == "Professor Jon")
        {
            if (Conditions.IsTrue(ConditionTypes.HasReceivedStarter))
            {
                Typewriter.TypeLine("Professor Jon is busy right now and doesn't have anything else for you.");
                States.ChangeState(StateTypes.Exploring);
                return;
            }
            Typewriter.TypeLine("Jon: Ah, shit! You're just in time, kid! *burp* I've been up all damn night coding these fuckin' Pals into existence! *burp* They're wild, they're unstable, but that's what makes 'em special! Now quit standing there like an idiot, do you want to pick your starter Pal or not?(yes/no)");
            pendingStarterChoice = "Professor Jon";
            // Print commands here for Professor Jon before awaiting choice
            Console.WriteLine(CommandList.conversationCommands);
        }
        // Nurse Noelia logic
        else if (npc.Name == "Nurse Noelia")
        {
            Typewriter.TypeLine("");
            pendingStarterChoice = "Nurse Noelia";
            // Print commands here for Nurse Noelia before awaiting choice
            Console.WriteLine(CommandList.conversationCommands);
        }
        // Default NPC
        else
        {
            Typewriter.TypeLine(npc.Description);
            // Print commands here for default NPCs before returning to exploration
            Console.WriteLine(CommandList.conversationCommands);
            States.ChangeState(StateTypes.Exploring);
        }
    }

    private static void ChooseStarter(Command command)
    {
        if (!awaitingStarterSelection || Conditions.IsTrue(ConditionTypes.HasReceivedStarter))
        {
            Typewriter.TypeLine("No starter selection is pending.");
            return;
        }
        string choice = command.Verb.Trim();
        string[] starters = { "Sandie", "Clyde Capybara", "Gloop Glorp" };
        int index = -1;
        if (int.TryParse(choice, out int num))
        {
            if (num >= 1 && num <= 3)
            {
                index = num - 1;
            }
        }
        if (index == -1)
        {
            Typewriter.TypeLine("Invalid choice. Please enter 1, 2, or 3.");
            return;
        }
        string selected = starters[index];
        var pal = Pals.GetPalByName(selected);
        if (pal == null)
        {
            Typewriter.TypeLine("That Pal is not available.");
            return;
        }
        Player.AddPal(pal);
        // Print the Pal's ASCII art before the confirmation message
        string art = pal.AsciiArt;
        if (!string.IsNullOrEmpty(art) && art.StartsWith("AsciiArt."))
        {
            var type = typeof(AsciiArt);
            var fieldName = art.Substring("AsciiArt.".Length);
            var field = type.GetField(fieldName);
            if (field != null)
                art = field.GetValue(null)?.ToString() ?? art;
            else
            {
                var propInfo = type.GetProperty(fieldName);
                if (propInfo != null)
                    art = propInfo.GetValue(null)?.ToString() ?? art;
            }
        }
        if (!string.IsNullOrEmpty(art))
            Console.WriteLine(art);
        Typewriter.TypeLine($"You chose {pal.Name} as your starter Pal!");
        awaitingStarterSelection = false;
        pendingStarterChoice = null;
        Conditions.ChangeCondition(ConditionTypes.HasReceivedStarter, true);

        Console.Clear();
        States.ChangeState(StateTypes.Exploring);
        Player.Look();
        Typewriter.TypeLine("Now go fight your first wild Pal!"); 
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
        Typewriter.TypeLine("Please choose your starter pal:\n1. Sandie\n2. Clyde Capybara\n3. Gloop Glorp");
    }
}