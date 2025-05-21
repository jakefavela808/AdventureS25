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
                AudioManager.StopAllSoundEffects();
                AudioManager.PlaySoundEffect("JonBusy.wav");
                Typewriter.TypeLineWithDuration("Jon: Hey kid, good to see ya! I'm swamped with some new Pal data right now, catch ya later!", 4000);
                States.ChangeState(StateTypes.Exploring);
                return;
            }
            AudioManager.StopAllSoundEffects();
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
                AudioManager.StopAllSoundEffects();
                AudioManager.PlaySoundEffect("WelcomeToPalCenter.wav");
                Typewriter.TypeLineWithDuration("Noelia: Welcome to the Pal Center! I see you're in need of some healing. Let me take care of that right away!", 5000);
                HealAllPals(); // Automatic healing, no yes/no prompt
                AudioManager.StopAllSoundEffects();
                AudioManager.PlaySoundEffect("HelpMatt.wav");
                Typewriter.TypeLineWithDuration("Noelia: Can you do me a favor? Take this potion to Matt in the creepy Old Cabin, I'm too scared to go there. He's been feeling under the weather and this should help him.", 8000);
                Player.AddItemToInventory("potion");
                Typewriter.TypeLine($"A potion has been added to your inventory.");
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
                AudioManager.StopAllSoundEffects();
                AudioManager.PlaySoundEffect("DeliverPotion.wav");
                Typewriter.TypeLineWithDuration("Noelia: Now go deliver this potion to Matt, he needs it badly!", 3000);
                return; 
            }
            else
            {
                if (Player.Pals.Any() && Player.Pals.Any(p => p.HP < p.MaxHP))
                {
                    AudioManager.StopAllSoundEffects();
                    AudioManager.PlaySoundEffect("WelcomeBack.wav");
                    Typewriter.TypeLineWithDuration("Noelia: Welcome back! Would you like me to heal your Pals today?", 3000);
                    pendingStarterChoice = "Nurse Noelia";
                    // Print commands here for Nurse Noelia before awaiting choice
                    Console.WriteLine(CommandList.conversationCommands); 
                }
                else if (Player.Pals.Any())
                {
                    AudioManager.StopAllSoundEffects();
                    AudioManager.PlaySoundEffect("PerfectShape.wav");
                    Typewriter.TypeLineWithDuration("Noelia: Your Pals are already in perfect shape! If you need anything, just let me know.", 3000);
                    Console.WriteLine(CommandList.conversationCommands);
                    States.ChangeState(StateTypes.Exploring);
                    Player.Look();
                }
                else
                {
                    AudioManager.StopAllSoundEffects();
                    AudioManager.PlaySoundEffect("NoPals.wav");
                    Typewriter.TypeLineWithDuration("Noelia: Oh, it seems you don't have any Pals with you right now. Come back when you do!", 4000);
                    Console.WriteLine(CommandList.conversationCommands);
                    States.ChangeState(StateTypes.Exploring);
                    Player.Look();
                }
            }
        }
        // Matt's Potion Delivery Logic
        else if (npc.Name == "Matt") 
        {
            if (Conditions.IsTrue(ConditionTypes.PlayerHasPotionForMatt))
            {
                // Logic for when player has the potion for Matt
                if (Player.HasItem("potion"))
                {
                    AudioManager.StopAllSoundEffects();
                    AudioManager.PlaySoundEffect("PotionDelivered.wav");
                    Typewriter.TypeLineWithDuration("Matt: Is that... a potion? From Noelia? Oh, thank goodness! I was starting to think I'd be stuck like this forever.", 7500);
                    Player.RemoveItemFromInventory("potion");
                    Conditions.ChangeCondition(ConditionTypes.PlayerHasPotionForMatt, false); // Quest to deliver potion completed
                    AudioManager.StopAllSoundEffects();
                    AudioManager.PlaySoundEffect("TakeThisKey.wav");
                    Typewriter.TypeLineWithDuration("Matt: Thanks, friend. I owe you one. Say... I know a secret. There's an old chest hidden in a cave not too far from here. Just head east from my cabin and take this key.", 10000);
                    Conditions.ChangeCondition(ConditionTypes.MattHasRevealedCave, true);
                    Player.AddItemToInventory("key");
                    Typewriter.TypeLine("A key has been added to your inventory.");

                    // Award XP for delivering the potion to Matt
                    int mattXpReward = 75;
                    if (Player.Pals.Any())
                    {
                        Pal? chosenPalForMattXp;
                        if (Player.Pals.Count == 1)
                        {
                            chosenPalForMattXp = Player.Pals[0];
                        }
                        else
                        {
                            // Use GetAvailablePals to only prompt for Pals that can receive XP
                            var availablePals = Player.GetAvailablePals();
                            if (availablePals.Count == 1)
                            {
                                chosenPalForMattXp = availablePals[0];
                            }
                            else if (availablePals.Count > 1)
                            {
                                chosenPalForMattXp = Player.PromptPalSelection(availablePals, "Which Pal should receive the XP from Matt?");
                            }
                            else // No available Pals (all fainted, though Player.Pals.Any() was true)
                            {
                                chosenPalForMattXp = null;
                            }
                        }

                        if (chosenPalForMattXp != null)
                        {
                            chosenPalForMattXp.AddExperience(mattXpReward);
                        }
                        else if (!Player.GetAvailablePals().Any()) // Double check if no pals were truly available
                        {
                             Typewriter.TypeLine($"(You would have gained {mattXpReward} XP for helping Matt, but all your Pals are fainted!)");
                        }
                        // If chosenPalForMattXp is null due to cancellation, no message is needed here.
                    }
                    else
                    {
                        Typewriter.TypeLine($"(You would have gained {mattXpReward} XP for helping Matt, but you have no Pals!)");
                    }

                    States.ChangeState(StateTypes.Exploring);
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
            // After Matt's interaction, show commands then return to exploring
            Console.WriteLine(CommandList.conversationCommands);
            States.ChangeState(StateTypes.Exploring);
            Player.Look();
            // No pending choice for Matt, so don't print conversation commands or set pendingStarterChoice (already handled)
        }
        // Trainer Saul Logic
        else if (npc.Name == "Trainer Saul")
        {
            if (Conditions.IsTrue(ConditionTypes.DefeatedTrainerSaul))
            {
                Typewriter.TypeLine("Trainer Saul: Grr... I'm training for our rematch! You got lucky last time!");
                States.ChangeState(StateTypes.Exploring);
                // Player.Look(); // Look is called later if no other interaction happens
                return;
            }

            Typewriter.TypeLine("Trainer Saul: You think you're tough enough to face me, rookie?");
            Typewriter.TypeLine("Trainer Saul challenges you to a battle!");
            AudioManager.PlaySoundEffect("TrainerBattleStart.wav"); 

            Game.ActiveTrainer = npc; 
            Game.ActiveTrainerParty = new List<Pal>
            {
                Pals.GetPalByName("Lostling")!,
                Pals.GetPalByName("Smiley")!,
            };

            foreach (var pal in Game.ActiveTrainerParty)
            {
                if (pal == null) continue; 
                pal.GetType().GetProperty("Level")!.SetValue(pal, 5, null);
                pal.MaxHP += 10 * 4; 
                pal.HP = pal.MaxHP;
                pal.BasicAttackUses = pal.MaxBasicAttackUses;
                pal.SpecialAttackUses = pal.MaxSpecialAttackUses;
            }
            Game.CurrentTrainerPalIndex = 0;

            Pal? firstSaulPal = Game.ActiveTrainerParty[Game.CurrentTrainerPalIndex];

            if (firstSaulPal != null && firstSaulPal.HP > 0) // Ensure Pal is conscious
            {
                // BattleManager.StartBattle will handle Player Pal selection and set state to Fighting.
                BattleManager.StartBattle(null, firstSaulPal, isTrainerBattle: true); 
                // After StartBattle, control should return to the main game loop to process battle commands.
                // The ConversationCommandHandler's job for initiating this specific Pal battle is done.
                return; 
            }
            else
            {
                Typewriter.TypeLine("Trainer Saul: Huh? My Pals aren't ready! Lucky for you..."); // Should not happen with this setup
                Game.ActiveTrainer = null; // Clear active trainer state
                Game.ActiveTrainerParty = null;
                States.ChangeState(StateTypes.Exploring);
                // Player.Look(); // Look is called later
                return;
            }
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
        AudioManager.StopAllSoundEffects();
        AudioManager.PlaySoundEffect("AmazingPick.wav");
        Typewriter.TypeLineWithDuration("Jon: Thats an amazing pick! Dont fuckin' abuse it or whatever take care of the little creature.", 4400);
        awaitingStarterSelection = false;
        pendingStarterChoice = null;
        AudioManager.PlaySoundEffect("LabChest.wav");
        Typewriter.TypeLineWithDuration("Jon: Also, take this key. Try to open the chest in my Lab its been locked for a while.", 4100);
        Player.AddItemToInventory("key");
        Typewriter.TypeLine("A key has been added to your inventory.");
        AudioManager.PlaySoundEffect("FightFirstPal.wav");
        Typewriter.TypeLineWithDuration("Jon: Now go fight your first Pal!", 1650);
        Conditions.ChangeCondition(ConditionTypes.HasReceivedStarter, true);
        States.ChangeState(StateTypes.Exploring);
        Player.Look();
        AudioManager.StopAllSoundEffects();
         
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
        AudioManager.StopAllSoundEffects();
        AudioManager.PlaySoundEffect("PickAnOption.wav");
        Typewriter.TypeLineWithDuration("\nPlease choose your starter pal:\n1. Sandie\n2. Clyde Capybara\n3. Gloop Glorp", 5000);
    }
}