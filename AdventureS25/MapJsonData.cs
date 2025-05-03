namespace AdventureS25;

public class MapJsonData
{
    public List<LocationJsonData> Locations { get; set; }
    public string StartLocation { get; set; }
    public string? StartupAudioFile { get; set; } // Optional startup audio
}