namespace AdventureS25;

public class Pal
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string InitialDescription { get; set; }
    public bool IsAcquirable { get; set; }
    public string AsciiArt { get; set; }
    public string Location { get; set; }
    public List<string> Moves { get; set; }

    public int HP { get; set; }
    public int MaxHP { get; set; }

    public Pal(string name, string description, string initialDescription, bool isAcquirable, string asciiArt, string location, List<string> moves, int maxHP = 50)
    {
        Name = name;
        Description = description;
        InitialDescription = initialDescription;
        IsAcquirable = isAcquirable;
        AsciiArt = asciiArt;
        Location = location;
        Moves = moves;
        MaxHP = maxHP;
        HP = maxHP;
    }

    public string GetLocationDescription()
    {
        return $"{AsciiArt}\n{InitialDescription}";
    }
}
