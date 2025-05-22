namespace AdventureS25;

using AdventureS25;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class Player
{
    public static Location? CurrentLocation;
    public static List<Item> Inventory = new List<Item>();
    public static List<Pal> Pals = new List<Pal>();

    public static void Initialize()
    {
        Inventory = new List<Item>();
        Pals = new List<Pal>();
        CurrentLocation = Map.StartLocation; 
    }

    public static void Move(Command command)
    {
        if (CurrentLocation == null)
        {
            Typewriter.TypeLine("Error: Current location is not set. Cannot move.");
            return;
        }

        if (CurrentLocation == Map.StartLocation && !Conditions.IsTrue(ConditionTypes.HasReadNote))
        {
            Typewriter.TypeLine("You should read the note before leaving!");
            Console.Clear();
            Look();
            return;
        }
        if (CurrentLocation.CanMoveInDirection(command))
        {
            Location? oldLocation = CurrentLocation;
            Location? newLocation = CurrentLocation.GetLocationInDirection(command);
            if (newLocation == null)
            {
                Typewriter.TypeLine("Error: Tried to move to a null location.");
                Console.Clear();
                Look();
                return;
            }

            if (oldLocation != null)
            {
                oldLocation.ActiveWildPal = null;
            }

            CurrentLocation = newLocation;

            Game.TrySpawnWildPal(CurrentLocation); 

            Console.Clear();
            Console.WriteLine(CurrentLocation.GetDescription());

            PlayNarrativeIfNeeded(CurrentLocation);
            AudioManager.PlayLooping(CurrentLocation.AudioFile);

        }
        else
        {
            Typewriter.TypeLine("You can't move " + command.Noun + ".");
            Console.Clear();
            Look();
        }
    }

    public static string GetLocationDescription()
    {
        return CurrentLocation?.GetDescription() ?? "You are in an unknown void. Something is wrong.";
    }

    public static void Take(Command command)
    {
        if (CurrentLocation == null)
        {
            Typewriter.TypeLine("Error: Current location is not set. Cannot take items.");
            return;
        }
        Item? item = Items.GetItemByName(command.Noun);

        if (item == null)
        {
            Typewriter.TypeLine("I don't know what " + command.Noun + " is.");
            Console.Clear();
            Look();
        }
        else if (!CurrentLocation.HasItem(item))
        {
            Typewriter.TypeLine("There is no " + command.Noun + " here.");
            Console.Clear();
            Look();
        }
        else if (!item.IsTakeable)
        {
            Typewriter.TypeLine("The " + command.Noun + " can't be taked.");
            Console.Clear();
            Look();
        }
        else
        {
            Inventory.Add(item);
            CurrentLocation.RemoveItem(item);
            item.Pickup();
            Typewriter.TypeLine("You take the " + command.Noun + ".");
            Console.Clear();
            Look();
        }
    }

    public static void ShowInventory()
    {
        if (Inventory.Count == 0)
        {
            Typewriter.TypeLine("You are empty-handed.");
        }
        else
        {
            Typewriter.TypeLine("You are carrying:");
            foreach (Item item in Inventory)
            {
                string article = SemanticTools.CreateArticle(item.Name);
                Typewriter.TypeLine(article + " " + item.Name);
            }
        }
    }

    public static void ShowPals()
    {
        if (Pals.Count == 0)
        {
            Typewriter.TypeLine("You have no Pals yet.");
            if (CurrentLocation == null) 
            { 
                Console.WriteLine("Error: Current location is unknown."); 
                return; 
            }
            Console.Clear();
            Look();
        }
        else
        {
            Typewriter.TypeLine("Your Pals:");
            foreach (Pal pal in Pals)
            {
                Typewriter.TypeLine($"\n{pal.Name} - HP: {pal.HP}/{pal.MaxHP}");
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
                Typewriter.TypeLine($"{pal.Description}");
            }
        }
    }

    public static void Look()
    {
        Console.WriteLine(CurrentLocation?.GetDescription() ?? "You are in an unknown void. Something is wrong.");
    }

    public static void Drop(Command command)
    {       
        if (CurrentLocation == null)
        {
            Typewriter.TypeLine("Error: Current location is not set. Cannot drop items.");
            return;
        }
        Item? item = Items.GetItemByName(command.Noun);

        if (item == null)
        {
            string article = SemanticTools.CreateArticle(command.Noun);
            Typewriter.TypeLine("I don't know what " + article + " " + command.Noun + " is.");
            Console.Clear();
            Look();
        }
        else if (!Inventory.Contains(item))
        {
            Typewriter.TypeLine("You're not carrying the " + command.Noun + ".");
            Console.Clear();
            Look();
        }
        else
        {
            Inventory.Remove(item);
            CurrentLocation.AddItem(item);
            Typewriter.TypeLine("You drop the " + command.Noun + ".");
            Console.Clear();
            Look();
        }

    }

    public static void Drink(Command command)
    {
        if (command.Noun == "beer")
        {
            Typewriter.TypeLine("** drinking beer");
            Conditions.ChangeCondition(ConditionTypes.IsDrunk, true);
            RemoveItemFromInventory("beer");
            AddItemToInventory("beer-bottle");
        }
    }

    public static void AddItemToInventory(string itemName)
    {
        Item? item = Items.GetItemByName(itemName);

        if (item == null)
        {
            return;
        }
        
        Inventory.Add(item);
    }

    public static void AddPal(Pal palTemplate)
    {
        if (palTemplate == null) return;

        Pal newPalInstance = palTemplate.Clone();

        if (!Pals.Contains(newPalInstance))
        {
            Pals.Add(newPalInstance);
        }
    }

    public static List<Pal> GetAvailablePals()
    {
        return Pals.Where(p => p.HP > 0).ToList();
    }
    public static Pal? PromptPalSelection(List<Pal> palList, string prompt)
    {
        if (palList == null || palList.Count == 0)
        {
            Typewriter.TypeLine("No available Pals to select.");
            return null;
        }
        while (true)
        {
            Typewriter.TypeLine(prompt);
            for (int i = 0; i < palList.Count; i++)
            {
                var pal = palList[i];
                Typewriter.TypeLine($"{i + 1}. {pal.Name} (HP: {pal.HP}/{pal.MaxHP})");
            }
            Typewriter.TypeLine("Enter the number of your choice:");
            string? input = CommandProcessor.GetInput();
            if (int.TryParse(input, out int selection) && selection >= 1 && selection <= palList.Count)
            {
                return palList[selection - 1];
            }
            Typewriter.TypeLine("Invalid selection. Please try again.");
        }
    }

    public static bool HasItem(string itemName)
    {
        return Inventory.Any(i => i.Name.ToLower() == itemName.ToLower());
    }

    public static void RemoveItemFromInventory(string itemName)
    {
        var itemToRemove = Inventory.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (itemToRemove != null)
        {
            Inventory.Remove(itemToRemove);
        }
    }

    public static void MoveToLocation(string locationName)
    {
        Location? loc = Map.GetLocationByName(locationName);
        if (loc != null)
        {
            CurrentLocation = loc;
            PlayNarrativeIfNeeded(CurrentLocation);
            AudioManager.PlayLooping(CurrentLocation.AudioFile);
            Look();
            AudioManager.Stop();
        }
        else
        {
            Typewriter.TypeLine($"Error: Location '{locationName}' not found. Cannot move.");
            Look();
        }
    }

    public static void PlayNarrativeIfNeeded(Location? location)
    {
        if (location == null) return;
        switch (location.Name)
        {
            case "Orange Town":
                AudioManager.PlaySoundEffect("OrangeTownNarrative.wav");
                break;
            case "Viridian City":
                AudioManager.PlaySoundEffect("CityNarrative.wav");
                break;
            case "Route 1":
                AudioManager.PlaySoundEffect("RouteOneNarrative.wav");
                break;
            case "Log Cabin":
                AudioManager.PlaySoundEffect("LogCabinNarrative.wav");
                break;
            case "Pal Center":
                AudioManager.PlaySoundEffect("PalCenterNarrative.wav");
                break;
            case "Cave":
                AudioManager.PlaySoundEffect("CaveNarrative.wav");
                break;
            case "Professor Jon's Lab":
                AudioManager.PlaySoundEffect("JonsLabNarrative.wav");
                break;
        }
    }

    public static void Read(Command command)
    {
        Item? noteItem = Items.GetItemByName("note");
        if (noteItem != null && Inventory.Contains(noteItem)) { 
            Console.Clear();
            Look();
            AudioManager.PlaySoundEffect("ReadingNote.wav");
            Typewriter.TypeLineWithDuration("Dear Adventurer,\n\nListen up fucker! I heard you're trying to become some kind of Pal Tamer or whatever. GOOD NEWS! I'm gonna help you not completely suck at it! I've been studying this AMAZING new Pal specimen that's perfect for beginners.\n\nGet your ass over to my Fusion Lab ASAP!!! Don't make me come find you, because I WILL, and you WON'T like it! This is important COMPUTER SCIENCE happening here!\n\nSincerely, \nProf. Jon (the smartest Computer Scientist in this dimension)", 26000);
            Inventory.Remove(noteItem);
            Conditions.ChangeCondition(ConditionTypes.HasReadNote, true);
            Console.Clear();
            Look();
        }
        else
        {
            Typewriter.TypeLine("You don't have a note to read.");
            Console.Clear();
            Look();
        }
    }

    public static bool DoFullPartyHeal()
    {
        if (Pals.Count == 0)
        {
            return false; 
        }

        foreach (var pal in Pals)
        {
            pal.HP = pal.MaxHP;
            pal.BasicAttackUses = pal.MaxBasicAttackUses;
            pal.SpecialAttackUses = pal.MaxSpecialAttackUses;
        }
        return true; 
    }
}