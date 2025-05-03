using System.Text.Json;

namespace AdventureS25;

using AdventureS25;

public static class Map
{
    private static Dictionary<string, Location> nameToLocation = 
        new Dictionary<string, Location>();
    public static Location StartLocation;
    public static string? StartupAudioFile { get; private set; } // Optional startup audio
    
    public static void Initialize()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "Map.json");
        string rawText = File.ReadAllText(path);
        
        MapJsonData data = JsonSerializer.Deserialize<MapJsonData>(rawText);

        // make all the locations
        Dictionary<string, Location> locations = new Dictionary<string, Location>();
        foreach (LocationJsonData location in data.Locations)
        {
            string? audioFile = location.AudioFile; // Capture per-location audio file name
            string asciiArt = null;
            if (!string.IsNullOrEmpty(location.AsciiArt))
            {
                string artKey = location.AsciiArt;
                // If the value is like 'AsciiArt.cityLocation', extract 'cityLocation'
                int dotIdx = artKey.IndexOf('.');
                if (dotIdx >= 0 && dotIdx < artKey.Length - 1)
                {
                    artKey = artKey.Substring(dotIdx + 1);
                }
                // Support both field and property lookup
                var asciiArtField = typeof(AsciiArt).GetField(artKey, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (asciiArtField != null)
                {
                    asciiArt = asciiArtField.GetValue(null) as string;
                }
                else
                {
                    var asciiArtProp = typeof(AsciiArt).GetProperty(artKey, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (asciiArtProp != null)
                    {
                        asciiArt = asciiArtProp.GetValue(null) as string;
                    }
                }
            }
            Location newLocation = AddLocation(location.Name, location.Description, asciiArt, audioFile);
            locations.Add(location.Name, newLocation);
        }
        
        // setup all the connections
        foreach (LocationJsonData location in data.Locations)
        {
            Location currentLocation = locations[location.Name];
            foreach (KeyValuePair<string,string> connection in location.Connections)
            {
                string direction = connection.Key.ToLower();
                string destination = connection.Value;

                if (nameToLocation.ContainsKey(destination))
                {
                    Location destinationLocation = nameToLocation[destination];
                    currentLocation.AddConnection(direction, destinationLocation);
                }
                else
                {
                    Typewriter.TypeLine("Unknown destination: " + destination);
                }
            }
        }

        StartupAudioFile = data.StartupAudioFile; // Read startup audio filename

        if (locations.TryGetValue(data.StartLocation, out Location startLocation))
        {
            StartLocation = startLocation;
        }
        else
        {
            Typewriter.TypeLine("StartLocation not found in Map.json");
        }
    }

    private static Location AddLocation(string locationName, string locationDescription, string asciiArt = null, string? audioFile = null)
    {
        Location newLocation = new Location(locationName, locationDescription, asciiArt);
        newLocation.AudioFile = audioFile; // Assign audio file reference
        nameToLocation.Add(locationName, newLocation);
        return newLocation;
    }
    
    public static void AddItem(string itemName, string locationName)
    {
        // find out which Location is named locationName
        Location location = GetLocationByName(locationName);
        Item item = Items.GetItemByName(itemName);
        
        // add the item to the location
        if (item != null && location != null)
        {
            location.AddItem(item);
        }
    }
    
    public static void RemoveItem(string itemName, string locationName)
    {
        // find out which Location is named locationName
        Location location = GetLocationByName(locationName);
        Item item = Items.GetItemByName(itemName);
        
        // remove the item to the location
        if (item != null && location != null)
        {
            location.RemoveItem(item);
        }
    }

    public static Location GetLocationByName(string locationName)
    {
        if (nameToLocation.ContainsKey(locationName))
        {
            return nameToLocation[locationName];
        }
        else
        {
            return null;
        }
    }

    public static void AddConnection(string startLocationName, string direction, 
        string endLocationName)
    {
        // get the location objects based on the names
        Location start = GetLocationByName(startLocationName);
        Location end = GetLocationByName(endLocationName);
        
        // if the locations don't exist
        if (start == null || end == null)
        {
            Typewriter.TypeLine("Tried to create a connection between unknown locations: " +
                              startLocationName + " and " + endLocationName);
            return;
        }
            
        // create the connection
        start.AddConnection(direction, end);
    }

    public static void RemoveConnection(string startLocationName, string direction)
    {
        Location start = GetLocationByName(startLocationName);
        
        if (start == null)
        {
            Typewriter.TypeLine("Tried to remove a connection from an unknown location: " +
                              startLocationName);
            return;
        }

        start.RemoveConnection(direction);
    }
}