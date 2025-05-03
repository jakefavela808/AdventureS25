namespace AdventureS25;

using AdventureS25;

public class Location
{
    public List<Pal> pals = new List<Pal>();
    private List<Npc> npcs = new List<Npc>();

    public void AddPal(Pal pal) { if (!pals.Contains(pal)) pals.Add(pal); }
    public void RemovePal(Pal pal) { pals.Remove(pal); }
    public IReadOnlyList<Pal> GetPals() => pals.AsReadOnly();

    public void AddNpc(Npc npc) { if (!npcs.Contains(npc)) npcs.Add(npc); }
    public void RemoveNpc(Npc npc) { npcs.Remove(npc); }
    public IReadOnlyList<Npc> GetNpcs() => npcs.AsReadOnly();

    private string? asciiArt;

    // Optional audio file name for this location
    public string? AudioFile { get; set; }

    private string name;
    public string Description;
    
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
        // Add explore commands after art, before description
        fullDescription += "\n" + CommandList.exploreCommands + "\n";

        fullDescription += Description;

        // Show NPCs in the location (no ASCII art, just initial description)
        foreach (Npc npc in npcs)
        {
            // Simply state that the NPC is present
            if (npc != null) // Basic check to ensure npc object exists
            {
                fullDescription += $"\n{npc.Name} is here!";
            }
        }
        // Show only ONE random Pal's initial description (no ASCII art)
        if (pals.Count > 0)
        {
            var random = new Random();
            var pal = pals[random.Next(pals.Count)];
            if (!string.IsNullOrEmpty(pal.InitialDescription))
            {
                fullDescription += "\n" + pal.InitialDescription;
            }
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