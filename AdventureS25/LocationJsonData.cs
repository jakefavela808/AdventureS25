namespace AdventureS25;

public class LocationJsonData
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, string>? Connections { get; set; }
    public string? AsciiArt { get; set; }
    public string? AudioFile { get; set; }
}