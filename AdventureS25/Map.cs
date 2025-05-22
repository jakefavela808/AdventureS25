using System.Text.Json;

namespace AdventureS25;

using AdventureS25;

public static class Map
{
    private static Dictionary<string, Location> nameToLocation = 
        new Dictionary<string, Location>();
    public static Location? StartLocation { get; set; }
    public static string? StartupAudioFile { get; private set; }
    
    public static void Initialize()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "Map.json");
        string rawText = File.ReadAllText(path);
        
        MapJsonData? data = JsonSerializer.Deserialize<MapJsonData>(rawText);

        if (data == null || data.Locations == null)
        {
            Typewriter.TypeLine("[ERROR] Map.json could not be loaded or is empty/malformed.");
            return;
        }

        Dictionary<string, Location> locations = new Dictionary<string, Location>();
        foreach (LocationJsonData locationData in data.Locations)
        {
            if (locationData.Name == null || locationData.Description == null)
            {
                Typewriter.TypeLine("[WARNING] Location found with missing Name or Description in Map.json. Skipping.");
                continue;
            }

            string? audioFile = locationData.AudioFile;
            string? asciiArtString = null;
            if (!string.IsNullOrEmpty(locationData.AsciiArt))
            {
                string artKey = locationData.AsciiArt;
                int dotIdx = artKey.IndexOf('.');
                if (dotIdx >= 0 && dotIdx < artKey.Length - 1)
                {
                    artKey = artKey.Substring(dotIdx + 1);
                }
                var asciiArtField = typeof(AsciiArt).GetField(artKey, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (asciiArtField != null)
                {
                    asciiArtString = asciiArtField.GetValue(null) as string;
                }
                else
                {
                    var asciiArtProp = typeof(AsciiArt).GetProperty(artKey, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (asciiArtProp != null)
                    {
                        asciiArtString = asciiArtProp.GetValue(null) as string;
                    }
                }
            }
            Location newLocation = AddLocation(locationData.Name, locationData.Description, asciiArtString, audioFile);
            locations[locationData.Name] = newLocation;
        }
        foreach (LocationJsonData locationData in data.Locations)
        {
            if (locationData.Name == null || !locations.ContainsKey(locationData.Name) || locationData.Connections == null)
            {
                continue; 
            }
            Location currentLocation = locations[locationData.Name];
            foreach (KeyValuePair<string,string> connection in locationData.Connections)
            {
                string direction = connection.Key.ToLower();
                string destinationName = connection.Value;

                if (locations.TryGetValue(destinationName, out Location? destinationLocation))
                {
                    currentLocation.AddConnection(direction, destinationLocation);
                }
                else
                {
                    Typewriter.TypeLine("Unknown destination: " + destinationName + " for location " + locationData.Name);
                }
            }
        }

        StartupAudioFile = data.StartupAudioFile;

        if (data.StartLocation != null && locations.TryGetValue(data.StartLocation, out Location? foundStartLocation))
        {
            StartLocation = foundStartLocation;
        }
        else if (locations.Count > 0)
        {
            Typewriter.TypeLine(data.StartLocation == null ? "[WARNING] StartLocation not specified in Map.json. Defaulting to first loaded location." : "[WARNING] Specified StartLocation '" + (data.StartLocation ?? "null") + "' not found in Map.json. Defaulting to first loaded location.");
            StartLocation = locations.Values.First();
        }
        else
        {
            Typewriter.TypeLine("[ERROR] No locations loaded and no StartLocation specified. Map initialization failed. StartLocation will be null.");
        }
    }

    private static Location AddLocation(string locationName, string locationDescription, string? asciiArt = null, string? audioFile = null)
    {
        Location newLocation = new Location(locationName, locationDescription, asciiArt);
        newLocation.AudioFile = audioFile;
        nameToLocation.Add(locationName, newLocation);
        return newLocation;
    }
    
    public static void AddItem(string itemName, string locationName)
    {
        // find out which Location is named locationName
        Location location = GetLocationByName(locationName)!;
        Item item = Items.GetItemByName(itemName)!;
        
        // add the item to the location
        location.AddItem(item);
    }
    
    public static void RemoveItem(string itemName, string locationName)
    {
        // find out which Location is named locationName
        Location location = GetLocationByName(locationName)!;
        Item item = Items.GetItemByName(itemName)!;
        
        // remove the item to the location
        location.RemoveItem(item);
    }

    public static Location? GetLocationByName(string locationName)
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
        Location? start = GetLocationByName(startLocationName);
        Location? end = GetLocationByName(endLocationName);
        
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
        Location? start = GetLocationByName(startLocationName);
        
        if (start == null)
        {
            Typewriter.TypeLine("Tried to remove a connection from an unknown location: " +
                              startLocationName);
            return;
        }

        start.RemoveConnection(direction);
    }
}