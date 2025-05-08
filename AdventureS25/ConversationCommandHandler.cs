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

    private static string? pendingStarterChoice = null;
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
            Player.Look();
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
        Player.Look();
        
    }

    public static void Talk(Command command)
    {
        // Find the NPC at the current location
        var npcs = Player.CurrentLocation?.GetNpcs();
        if (npcs == null || npcs.Count == 0)
        {
            Typewriter.TypeLine("There is no one here to talk to.");
            Console.Clear();
            Player.Look();
            return;
        }
        // Talk to the first NPC
        Console.Clear();
        States.ChangeState(StateTypes.Talking);
        // Console.WriteLine(CommandList.conversationCommands); // Moved to after dialogue

        var npc = npcs[0];
        // Approach and display art
        Typewriter.TypeLine($"You approach {npc.Name}.");
        string? art = npc.AsciiArt;
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
        Typewriter.TypeLine(npc.Description ?? "");

        // Professor Jon logic
        if (npc.Name == "Professor Jon")
        {
            if (Conditions.IsTrue(ConditionTypes.HasReceivedStarter))
            {
                Typewriter.TypeLine("Professor Jon is busy right now and doesn't have anything else for you.");
                States.ChangeState(StateTypes.Exploring);
                return;
            }
            AudioManager.PlaySoundEffect("JonStarter.wav");
            Typewriter.TypeLineWithDuration("Jon: Ah, shit! You're just in time, kid! I've been up all damn night coding these fuckin' Pals into existence! They're wild, they're unstable, but that's what makes 'em special! Now quit standing there like an idiot, do you want to pick your starter Pal or not?", 12000);
            pendingStarterChoice = "Professor Jon";
            // Print commands here for Professor Jon before awaiting choice
            Console.WriteLine(CommandList.conversationCommands);
        }
        // Nurse Noelia logic
        else if (npc.Name == "Nurse Noelia")
        {
            if (Conditions.IsTrue(ConditionTypes.PlayerNeedsFirstHealFromNoelia))
            {
                AudioManager.PlaySoundEffect("WelcomeToPalCenter.wav");
                Typewriter.TypeLineWithDuration("Noelia: Welcome to the Pal Center! I see you're in need of some healing. Let me take care of that right away!", 5000);
                HealAllPals(); // Automatic healing, no yes/no prompt
                AudioManager.PlaySoundEffect("HelpMatt.wav");
                Typewriter.TypeLineWithDuration("Noelia: Can you do me a favor? Take this potion to Matt in the creepy Old Cabin, I'm too scared to go there. He's been feeling under the weather and this should help him.", 8000);
                Player.AddItemToInventory("potion");
                Conditions.ChangeCondition(ConditionTypes.PlayerNeedsFirstHealFromNoelia, false);
                Conditions.ChangeCondition(ConditionTypes.PlayerHasPotionForMatt, true); // Player now has the potion for Matt

                // Award XP for completing Noelia's initial interaction
                int noeliaXpReward = 50;
                if (Player.Pals.Any())
                {
                    Pal? chosenPal;
                    if (Player.Pals.Count == 1)
                    {
                        chosenPal = Player.Pals[0];
                    }
                    else
                    {
                        chosenPal = Player.PromptPalSelection(Player.Pals, "Which Pal should receive the XP?");
                    }
                    if (chosenPal != null)
                    {
                        chosenPal.AddExperience(noeliaXpReward);
                    }
                }
                else
                {
                    Typewriter.TypeLine($"(You would have gained {noeliaXpReward} XP, but you have no Pals!)");
                }
                States.ChangeState(StateTypes.Exploring); 
                Player.Look(); 
                AudioManager.PlaySoundEffect("DeliverPotion.wav");
                Typewriter.TypeLineWithDuration("Noelia: Now go deliver this potion to Matt, he needs it badly!", 3000);
                return; 
            }
            else
            {
                if (Player.Pals.Any() && Player.Pals.Any(p => p.HP < p.MaxHP))
                {
                    AudioManager.PlaySoundEffect("WelcomeBack.wav");
                    Typewriter.TypeLineWithDuration("Noelia: Welcome back! Would you like me to heal your Pals today?", 3000);
                    pendingStarterChoice = "Nurse Noelia";
                }
                else if (Player.Pals.Any())
                {
                    AudioManager.PlaySoundEffect("PerfectShape.wav");
                    Typewriter.TypeLineWithDuration("Noelia: Your Pals are already in perfect shape! If you need anything, just let me know.", 3000);
                }
                else
                {
                    AudioManager.PlaySoundEffect("NoPals.wav");
                    Typewriter.TypeLineWithDuration("Noelia: Oh! It looks like you don't have any Pals with you yet. Come back when you do, and I'll take good care of them!", 6000);
                    Console.Clear();
                    States.ChangeState(StateTypes.Exploring);
                    Player.Look();
                    return;
                }
            }
        }
        // Matt's Potion Delivery Logic
        else if (npc.Name == "Matt") 
        {
            if (Conditions.IsTrue(ConditionTypes.PlayerHasPotionForMatt))
            {
                if (Player.HasItem("potion"))
                {
                    Typewriter.TypeLine("Matt: *cough* Is that... a potion? From Noelia? Oh, thank goodness! I was starting to think I'd be stuck like this forever.");
                    Player.RemoveItemFromInventory("potion");
                    Conditions.ChangeCondition(ConditionTypes.PlayerHasPotionForMatt, false); // Quest to deliver potion completed
                    Typewriter.TypeLine("Matt: Thanks, friend. I owe you one. Say... I know a secret. There's an old chest hidden in a cave not too far from here. Might be something good in it for ya. Just head east from my cabin, can't miss it.");
                    Conditions.ChangeCondition(ConditionTypes.MattHasRevealedCave, true);

                    // Award XP for delivering the potion to Matt
                    int mattXpReward = 75;
                    if (Player.Pals.Any())
                    {
                        Player.Pals[0].AddExperience(mattXpReward);
                        Typewriter.TypeLine($"(Your team gained {mattXpReward} XP for helping Matt!)");
                    }
                    else
                    {
                        Typewriter.TypeLine($"(You would have gained {mattXpReward} XP, but you have no Pals!)");
                    }
                }
                else
                {
                    Typewriter.TypeLine("Matt: Noelia said you might have something for me, but it looks like you don't have that potion on you right now. Did you lose it?");
                }
            }
            else
            {
                // Default dialogue if player doesn't have the potion task or has already completed it.
                Typewriter.TypeLine("Matt: Urgh... just trying to rest here. This old cabin isn't doing my cough any favors.");
            }
            // After Matt's interaction, return to exploring
            States.ChangeState(StateTypes.Exploring);
            Player.Look();
            // No pending choice for Matt, so don't print conversation commands or set pendingStarterChoice
        }
        // Default NPC
        else
        {
            Typewriter.TypeLine(npc.Description ?? "");
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
        States.ChangeState(StateTypes.Exploring);
        Player.Look();
        AudioManager.PlaySoundEffect("FightFirstPal.wav");
        Typewriter.TypeLine("Jon: Now go fight your first Pal!"); 
    }

    private static void HealAllPals()
    {
        if (Player.DoFullPartyHeal()) // Call the new Player method
        {
            Typewriter.TypeLine("All your Pals have been fully healed!");
        }
        else
        {
            Typewriter.TypeLine("You have no Pals to heal.");
        }
    }

    private static void PromptStarterSelection()
    {
        AudioManager.PlaySoundEffect("PickAnOption.wav");
        Typewriter.TypeLine("\nPlease choose your starter pal:\n1. Sandie\n2. Clyde Capybara\n3. Gloop Glorp");
    }
}