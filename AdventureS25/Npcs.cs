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
        NpcsJsonData? data = JsonSerializer.Deserialize<NpcsJsonData>(rawText);

        if (data == null || data.NPCs == null) 
        {
            Typewriter.TypeLine("[ERROR] NPCs.json could not be loaded or is empty/malformed.");
            return;
        }

        foreach (var npc in data.NPCs)
        {
            if (string.IsNullOrEmpty(npc.Name))
            {
                Typewriter.TypeLine("[WARNING] NPC found with no name in NPCs.json. Skipping.");
                continue;
            }
            nameToNpc[npc.Name] = npc;

            if (string.IsNullOrEmpty(npc.Location))
            {
                Typewriter.TypeLine($"[INFO] NPC '{npc.Name}' does not have a location specified in NPCs.json and will not be placed in the world.");
                continue; 
            }

            var location = Map.GetLocationByName(npc.Location);
            if (location != null)
            {
                location.AddNpc(npc);
            }
            else
            {
                Typewriter.TypeLine($"[WARNING] Location '{npc.Location}' for NPC '{npc.Name}' not found. NPC not placed in world.");
            }
        }
    }

    public static Npc? GetNpcByName(string name) =>
        nameToNpc.TryGetValue(name, out var npc) ? npc : null;
}
