namespace AdventureS25;

using System; 
using AdventureS25;
using System.Linq;

public static class ExplorationCommandHandler
{
    private static Random random = new Random(); 
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
            {"pals", ShowPals},
            {"open", HandleOpen}, 
            {"mute", HandleMute},
            {"unmute", HandleUnmute}   
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
        var npcs = Player.CurrentLocation?.GetNpcs();
        if (npcs != null && npcs.Any(npc => npc.Name == "Professor Jon") && Conditions.IsTrue(ConditionTypes.HasReceivedStarter))
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

    private static void HandleMute(Command command)
    {
        if (AudioManager.IsMuted)
        {
            Typewriter.TypeLine("Audio is already muted.");
        }
        else
        {
            AudioManager.ToggleMute(); // This will mute and print "Audio muted."
        }
    }

    private static void HandleUnmute(Command command)
    {
        if (!AudioManager.IsMuted)
        {
            Typewriter.TypeLine("Audio is already unmuted.");
        }
        else
        {
            AudioManager.ToggleMute(); // This will unmute and print "Audio unmuted."
        }
    }

    private static void HandleOpen(Command command)
    {
        // Only allow opening a chest if the command is exactly 'open chest'.
        // If the noun is missing or not 'chest', do nothing (let the parser/validator/game handle it).
        if (string.IsNullOrWhiteSpace(command.Noun) || command.Noun.ToLower() != "chest")
        {
            return;
        }

        // Only proceed if the noun is exactly 'chest'.
        var chestItem = Player.CurrentLocation?.Items?.FirstOrDefault(item => item.Name.ToLower() == "chest");

        if (chestItem != null)
        {
            // For simplicity, we remove the chest. 
            // A more complex system might mark it as 'opened'.
            Player.CurrentLocation?.RemoveItem(chestItem); // Avoid null dereference

            int numPotions = random.Next(0, 3); // 0, 1, or 2 potions
            int numTreats = random.Next(0, 4);  // 0, 1, 2, or 3 treats
            int gainedXp = random.Next(25, 76); // 25 to 75 XP
            string lootMessage = "You find: ";
            if (numPotions == 0 && numTreats == 0 && gainedXp == 0)
            {
                lootMessage += "\nIt's empty!";
            }
            else
            {
                if (numPotions > 0)
                {
                    for (int i = 0; i < numPotions; i++)
                    {
                        Player.AddItemToInventory("potion");
                    }
                    lootMessage += $"\n- {numPotions} potion(s)";
                }
                if (numTreats > 0)
                {
                    for (int i = 0; i < numTreats; i++)
                    {
                        Player.AddItemToInventory("treat");
                    }
                    lootMessage += $"\n- {numTreats} treat(s)";
                }
                if (gainedXp > 0)
                {
                    if (Player.Pals.Any()) // Check if the player has any Pals
                    {
                        Player.Pals[0].AddExperience(gainedXp); // Give XP to the first Pal
                        // lootMessage += $"\n- {Player.Pals[0].Name} gained {gainedXp} XP"; // Redundant with Pal.AddExperience message
                    }
                    else
                    {
                        lootMessage += $"\n- {gainedXp} XP (You have no Pals to gain this XP!)";
                    }
                }
            }
            Typewriter.TypeLine("Opening chest...");
            ChestAnimation();
            Typewriter.TypeLine($"{lootMessage}");
        }
        else
        {
            Typewriter.TypeLine("There is no unopened chest here.");
            Console.Clear();
            Player.Look();
        }
        // Consider if Player.Look() is needed here or if the loot message is enough.
        // For now, let the loot message be the primary feedback.
    }

    public static void ChestAnimation()
    {
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(AsciiArt.chest);
            Thread.Sleep(100);
            Console.Clear();
            Player.Look();
            Console.WriteLine(AsciiArt.chestInverted);
            Thread.Sleep(100);
            Console.Clear();
            Player.Look();
        }
    }
}
