using System.Text.Json;
using System.IO;
using System.Collections.Generic;

namespace AdventureS25;

public static class Pals
{
    private static Dictionary<string, Pal> nameToPal = new();

    public static void Initialize()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "Pals.json");
        string rawText = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<PalsJsonData>(rawText);

        if (data == null || data.Pals == null)
        {
            Console.WriteLine("[ERROR] Pals.json could not be loaded or is empty.");
            return;
        }

        foreach (var pal in data.Pals)
        {
            if (pal.Name == null)
            {
                Console.WriteLine("[WARNING] Pal found with no name in Pals.json. Skipping.");
                continue;
            }
            nameToPal[pal.Name] = pal;

            if (pal.Location == null)
            {
                // Decide if pals must have a location or if this is a valid state (e.g. starter pal)
                // Console.WriteLine($"[INFO] Pal '{pal.Name}' does not have an initial location specified.");
                continue; // Or handle differently
            }

            var location = Map.GetLocationByName(pal.Location);
            if (location != null)
                location.AddPal(pal);
            else
            {
                Console.WriteLine($"[WARNING] Location '{pal.Location}' for Pal '{pal.Name}' not found.");
            }
        }
    }

    public static Pal? GetPalByName(string name) =>
        nameToPal.TryGetValue(name, out var pal) ? pal : null;
}
