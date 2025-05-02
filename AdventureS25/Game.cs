namespace AdventureS25;

public static class Game
{
    public static void PlayGame()
    {
        Console.Clear();
        Initialize();
        bool validMenuChoice = false;
        while (!validMenuChoice)
        {
            Console.Clear();
            Console.WriteLine(AsciiArt.titleAndLogo);
            Typewriter.TypeLine("1. Start Game\n2. Exit");
            string mainMenuInput = CommandProcessor.GetInput();

            if (mainMenuInput == "1")
            {
                validMenuChoice = true;
                States.ChangeState(StateTypes.Exploring);
                Console.WriteLine(Player.GetLocationDescription());
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
        Player.Initialize();
    }
}