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
        foreach (var pal in data.Pals)
        {
            nameToPal[pal.Name] = pal;
            var location = Map.GetLocationByName(pal.Location);
            if (location != null)
                location.AddPal(pal);
        }
    }

    public static Pal GetPalByName(string name) =>
        nameToPal.TryGetValue(name, out var pal) ? pal : null;
}
