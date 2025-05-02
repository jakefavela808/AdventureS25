namespace AdventureS25;

public class Npc
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string InitialDescription { get; set; }
    public bool IsInteractable { get; set; }
    public string AsciiArt { get; set; }
    public string Location { get; set; }

    public Npc(string name, string description, string initialDescription, bool isInteractable, string asciiArt, string location)
    {
        Name = name;
        Description = description;
        InitialDescription = initialDescription;
        IsInteractable = isInteractable;
        AsciiArt = asciiArt;
        Location = location;
    }

    public string GetLocationDescription()
    {
        return $"{AsciiArt}\n{InitialDescription}";
    }
}
