namespace AdventureS25;

using System.IO;

public static class Game
{
    public static void PlayGame()
    {
        Initialize();
        AudioManager.PlayOnce(Map.StartupAudioFile); // Play startup audio
        bool validMenuChoice = false;
        while (!validMenuChoice)
        {
            Console.Clear();
            Console.WriteLine(AsciiArt.titleAndLogo);
            Typewriter.TypeLine("1. Start Game\n2. Exit");
            string mainMenuInput = CommandProcessor.GetInput();

            if (mainMenuInput == "1")
            {
                AudioManager.Stop(); // Stop startup audio if it was playing
                validMenuChoice = true;
                States.ChangeState(StateTypes.Exploring);
                Console.WriteLine(Player.GetLocationDescription());
                AudioManager.PlayLooping(Player.CurrentLocation?.AudioFile); // Play starting location audio
                bool isPlaying = true;
                while (isPlaying)
                {
                    Command command = CommandProcessor.Process();
                    if (command.IsValid)
                    {
                        if (command.Verb == "exit")
                        {
                            Typewriter.TypeLine("Game Over!");
                            isPlaying = false;
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