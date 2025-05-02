namespace AdventureS25;

using AdventureS25;

public static class Debugger
{
    private static bool isActive = false;
    
    public static void Write(string message)
    {
        if (isActive)
        {
            Typewriter.TypeLine(message);
        }
    }

    public static void Tron()
    {
        isActive = true;
        Typewriter.TypeLine("Debugging on");
    }
    
    public static void Troff()
    {
        isActive = false;
        Typewriter.TypeLine("Debugging off");
    }
}