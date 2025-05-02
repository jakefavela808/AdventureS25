namespace AdventureS25;

using AdventureS25;

public class Location
{
    private string asciiArt;

    private string name;
    public string Description;
    
    public Dictionary<string, Location> Connections;
    public List<Item> Items = new List<Item>();
    
    public Location(string nameInput, string descriptionInput, string asciiArtInput = null)
    {
        name = nameInput;
        Description = descriptionInput;
        asciiArt = asciiArtInput;
        Connections = new Dictionary<string, Location>();
    }

    public void AddConnection(string direction, Location location)
    {
        Connections.Add(direction, location);
    }

    public bool CanMoveInDirection(Command command)
    {
        if (Connections.ContainsKey(command.Noun))
        {
            return true;
        }
        return false;
    }

    public Location GetLocationInDirection(Command command)
    {
        return Connections[command.Noun];
    }

    public string GetDescription()
    {
        // Add explore commands at the top
        string fullDescription = CommandList.exploreCommands + "\n";
        fullDescription += name + "\n";
        if (!string.IsNullOrEmpty(asciiArt))
        {
            fullDescription += asciiArt;
        }
        fullDescription += Description;

        foreach (Item item in Items)
        {
            fullDescription += "\n" + item.GetLocationDescription();
        }
        
        return fullDescription;
    }

    public void AddItem(Item item)
    {
        Debugger.Write("Adding item "+ item.Name + "to " + name);
        Items.Add(item);
    }

    public bool HasItem(Item itemLookingFor)
    {
        foreach (Item item in Items)
        {
            if (item.Name == itemLookingFor.Name)
            {
                return true;
            }
        }
        
        return false;
    }

    public void RemoveItem(Item item)
    {
        Items.Remove(item);
    }

    public void RemoveConnection(string direction)
    {
        if (Connections.ContainsKey(direction))
        {
            Connections.Remove(direction);
        }
    }
}