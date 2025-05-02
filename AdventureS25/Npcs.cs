using System.Text.Json;
using System.IO;
using System.Collections.Generic;

namespace AdventureS25;

public static class Npcs
{
    private static Dictionary<string, Npc> nameToNpc = new();

    public static void Initialize()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "NPCs.json");
        string rawText = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<NpcsJsonData>(rawText);
        foreach (var npc in data.Npcs)
        {
            nameToNpc[npc.Name] = npc;
            var location = Map.GetLocationByName(npc.Location);
            if (location != null)
                location.AddNpc(npc);
        }
    }

    public static Npc GetNpcByName(string name) =>
        nameToNpc.TryGetValue(name, out var npc) ? npc : null;
}
