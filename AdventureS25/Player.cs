namespace AdventureS25;

using AdventureS25;
using System.IO; // Potentially needed if AudioManager path logic changes

public static class Player
{
    public static Location CurrentLocation;
    public static List<Item> Inventory;
    public static List<Pal> Pals = new List<Pal>();

    public static void Initialize()
    {
        Inventory = new List<Item>();
        Pals = new List<Pal>();
        CurrentLocation = Map.StartLocation;
    }

    public static void Move(Command command)
    {
        // Only require HasReadNote condition
        if (CurrentLocation == Map.StartLocation && !Conditions.IsTrue(ConditionTypes.HasReadNote))
        {
            Typewriter.TypeLine("You should read the note before leaving!");
            Console.Clear();
            Look();
            return;
        }
        if (CurrentLocation.CanMoveInDirection(command))
        {
            AudioManager.Stop(); // Stop current location's audio
            CurrentLocation = CurrentLocation.GetLocationInDirection(command);
            Console.Clear();
            Console.WriteLine(CurrentLocation.GetDescription());
            AudioManager.PlayLooping(CurrentLocation.AudioFile); // Play new location's audio
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
        return CurrentLocation.GetDescription();
    }

    public static void Take(Command command)
    {
        // figure out which item to take: turn the noun into an item
        Item item = Items.GetItemByName(command.Noun);

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
            Console.Clear();
            Look();
        }
        else
        {
            Typewriter.TypeLine("Your Pals:");
            foreach (Pal pal in Pals)
            {
                Typewriter.TypeLine($"\n{pal.Name} - HP: {pal.HP}/{pal.MaxHP}");
                // Print resolved ASCII art above description
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
        Console.WriteLine(CurrentLocation.GetDescription());
    }

    public static void Drop(Command command)
    {       
        Item item = Items.GetItemByName(command.Noun);

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
        Item item = Items.GetItemByName(itemName);

        if (item == null)
        {
            return;
        }
        
        Inventory.Add(item);
    }

    public static void AddPal(Pal pal)
    {
        if (!Pals.Contains(pal))
        {
            Pals.Add(pal);
        }
    }

    public static bool HasItem(string itemName)
    {
        return Inventory.Any(i => i.Name.ToLower() == itemName.ToLower());
    }

    public static void RemoveItemFromInventory(string itemName)
    {
        Item item = Items.GetItemByName(itemName);
        if (item == null)
        {
            return;
        }
        Inventory.Remove(item);
    }

    public static void MoveToLocation(string locationName)
    {
        // look up the location object based on the name
        Location newLocation = Map.GetLocationByName(locationName);
        
        // if there's no location with that name
        if (newLocation == null)
        {
            Typewriter.TypeLine("Trying to move to unknown location: " + locationName + ".");
            Console.Clear();
            Look();
            return;
        }
            
        AudioManager.Stop(); // Stop current location's audio
        // set our current location to the new location
        CurrentLocation = newLocation;
        AudioManager.PlayLooping(CurrentLocation.AudioFile); // Play new location's audio
        // print out a description of the location
        Look();
    }

    public static void Read(Command command)
    {
        if (Inventory.Contains(Items.GetItemByName("note"))) {
    Console.Clear();
    Look();
    Typewriter.TypeLine("Dear Adventurer,\n\nListen up fucker! I heard you're trying to become some kind of Pal Tamer or whatever. GOOD NEWS! I'm gonna help you not completely suck at it! I've been studying this AMAZING new Pal specimen that's perfect for beginners.\n\nGet your ass over to my Fusion Lab ASAP!!! Don't make me come find you, because I WILL, and you WON'T like it! This is important COMPUTER SCIENCE happening here!\n\nSincerely, \nProf. Jon (the smartest Computer Scientist in this dimension)");
    Inventory.Remove(Items.GetItemByName("note"));
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
}