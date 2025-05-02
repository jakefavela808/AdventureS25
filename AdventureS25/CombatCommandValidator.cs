namespace AdventureS25;

public static class CombatCommandValidator
{
    public static bool IsValid(Command command)
    {
        if (command.Verb == "1" || command.Verb == "2" ||
            command.Verb == "3" || command.Verb == "4")
        {
            return true;
        }
        Typewriter.TypeLine("Valid commands are: 1, 2, 3, 4");
        return false;
    }
}