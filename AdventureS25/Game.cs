namespace AdventureS25;

using System.Collections.Generic;
using System.IO;

public static class Game
{
    public static Dictionary<string, List<string>> TamedPalsByLocation { get; set; } = new Dictionary<string, List<string>>();
    public static Npc? ActiveTrainer { get; set; }
    public static int CurrentTrainerPalIndex { get; set; }
    public static List<Pal>? ActiveTrainerParty { get; set; }

    public static bool ContinueMainLoop { get; set; } = true;

    public static void TrySpawnWildPal(Location location)
    {
        if (location == null || !location.PotentialPalNames.Any())
        {
            location.ActiveWildPal = null;
            return;
        }

        List<string> availableToSpawn = new List<string>(location.PotentialPalNames);

        if (!TamedPalsByLocation.ContainsKey(location.Name))
        {
            TamedPalsByLocation[location.Name] = new List<string>();
        }

        List<string> tamedInThisLocation = TamedPalsByLocation[location.Name];
        availableToSpawn.RemoveAll(palName => tamedInThisLocation.Contains(palName));

        if (!availableToSpawn.Any())
        {
            location.ActiveWildPal = null;
            return;
        }

        Random random = new Random();
        string palNameToSpawn = availableToSpawn[random.Next(availableToSpawn.Count)];

        Pal? palTemplate = Pals.GetPalByName(palNameToSpawn);

        if (palTemplate != null)
        {
            location.ActiveWildPal = palTemplate.Clone();
        }
        else
        {
            location.ActiveWildPal = null;
            Console.WriteLine($"[ERROR] Failed to find Pal template for '{palNameToSpawn}' during spawn attempt.");
        }
    }

    public static void PlayGame()
    {
        Initialize();
        ContinueMainLoop = true;
        AudioManager.PlayOnce(Map.StartupAudioFile);
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
                AudioManager.PlayLooping(Player.CurrentLocation?.AudioFile);
                while (ContinueMainLoop)
                {
                    Command command = CommandProcessor.Process();
                    if (!ContinueMainLoop) break;

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