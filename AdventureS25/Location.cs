namespace AdventureS25;

using AdventureS25;

public class Location
{
    public List<string> PotentialPalNames { get; set; } = new List<string>();
    public Pal? ActiveWildPal { get; set; }

    private List<Npc> npcs = new List<Npc>();

    public void AddNpc(Npc npc) { if (!npcs.Contains(npc)) npcs.Add(npc); }
    public void RemoveNpc(Npc npc) { npcs.Remove(npc); }
    public IReadOnlyList<Npc> GetNpcs() => npcs.AsReadOnly();

    private string? asciiArt;

    public string? AudioFile { get; set; }

    private string name;
    public string Description;
    public string Name { get { return name; } }
    
    public Dictionary<string, Location> Connections;
    public List<Item> Items = new List<Item>();
    
    public Location(string nameInput, string descriptionInput, string? asciiArtInput = null)
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
        string fullDescription = name + "\n";
        if (!string.IsNullOrEmpty(asciiArt))
        {
            fullDescription += asciiArt;
        }
        fullDescription += "\n" + CommandList.exploreCommands + "\n";

        fullDescription += Description;

        foreach (Npc npc in npcs)
        {
            if (npc != null)
            {
                fullDescription += $"\n{npc.Name} is here!";
            }
        }
        if (ActiveWildPal != null && !string.IsNullOrEmpty(ActiveWildPal.InitialDescription))
        {
            fullDescription += "\n" + ActiveWildPal.InitialDescription;
        }

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