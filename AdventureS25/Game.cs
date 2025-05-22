namespace AdventureS25;

using System.Collections.Generic;
using System.IO;

public static class Game
{
    // Tracks Pal names (species) that have been tamed from a specific location name.
    public static Dictionary<string, List<string>> TamedPalsByLocation { get; set; } = new Dictionary<string, List<string>>();

    // Fields for managing active trainer battle state
    public static Npc? ActiveTrainer { get; set; }
    public static int CurrentTrainerPalIndex { get; set; }
    public static List<Pal>? ActiveTrainerParty { get; set; }

    public static bool ContinueMainLoop { get; set; } = true;

    public static void TrySpawnWildPal(Location location)
    {
        if (location == null || !location.PotentialPalNames.Any())
        {
            location.ActiveWildPal = null; // Ensure no active pal if no potential spawns
            return;
        }

        List<string> availableToSpawn = new List<string>(location.PotentialPalNames);

        // Check if this location has a list of tamed Pals; if not, create it.
        if (!TamedPalsByLocation.ContainsKey(location.Name))
        {
            TamedPalsByLocation[location.Name] = new List<string>();
        }

        List<string> tamedInThisLocation = TamedPalsByLocation[location.Name];

        // Filter out already tamed species for this location
        availableToSpawn.RemoveAll(palName => tamedInThisLocation.Contains(palName));

        if (!availableToSpawn.Any())
        {
            location.ActiveWildPal = null; // No available (untamed) Pals to spawn
            return;
        }

        // Select a random Pal from the filtered list
        Random random = new Random();
        string palNameToSpawn = availableToSpawn[random.Next(availableToSpawn.Count)];

        // Get the Pal template/prototype
        Pal? palTemplate = Pals.GetPalByName(palNameToSpawn);

        if (palTemplate != null)
        {
            location.ActiveWildPal = palTemplate.Clone(); // Spawn a new instance
        }
        else
        {
            location.ActiveWildPal = null; // Should not happen if PotentialPalNames are valid
            Console.WriteLine($"[ERROR] Failed to find Pal template for '{palNameToSpawn}' during spawn attempt.");
        }
    }

    public static void PlayGame()
    {
        Initialize();
        ContinueMainLoop = true; // Ensure it's true when game starts
        AudioManager.PlayOnce(Map.StartupAudioFile); // Play startup audio
        bool validMenuChoice = false;
        while (!validMenuChoice && ContinueMainLoop)
        {
            Console.Clear();
            Console.WriteLine(AsciiArt.titleAndLogo);
            Typewriter.TypeLine("1. Start Game\n2. Exit");
            string mainMenuInput = CommandProcessor.GetInput();

            if (mainMenuInput == "1")
            {
                AudioManager.Stop();
                AudioManager.PlaySoundEffect("Input.wav");
                validMenuChoice = true;
                States.ChangeState(StateTypes.Exploring);
                Console.WriteLine(Player.GetLocationDescription());
            
                Player.PlayNarrativeIfNeeded(Player.CurrentLocation);
                AudioManager.PlayLooping(Player.CurrentLocation?.AudioFile); // Play starting location audio
                while (ContinueMainLoop)
                {
                    Command command = CommandProcessor.Process();
                    if (!ContinueMainLoop) break; // Check flag after Process() in case it blocks and flag changes

                    if (command.IsValid)
                    {
                        if (command.Verb == "exit")
                        {
                            Typewriter.TypeLine("Game Over!");
                            ContinueMainLoop = false;
                        }
                        else
                        {
                            CommandHandler.Handle(command);
                        }
                    }
                    // If ContinueMainLoop was set to false by CommandHandler.Handle (e.g. via BattleManager), the loop will terminate.
                }
            }
            else if (mainMenuInput == "2")
            {
                Typewriter.TypeLine("Goodbye!");
                ContinueMainLoop = false;
                validMenuChoice = true;
            }
            else
            {
                Typewriter.TypeLine("Invalid choice. Please enter 1 or 2.");
            }
        }
    }

    private static void Initialize()
    {
        Conditions.Initialize();
        States.Initialize();
        Map.Initialize();
        Items.Initialize();
        Npcs.Initialize();
        Pals.Initialize();
        Player.Initialize();
    }
}