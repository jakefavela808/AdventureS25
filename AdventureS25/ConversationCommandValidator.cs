namespace AdventureS25;

using AdventureS25;

public static class ConversationCommandValidator
{
    public static bool IsValid(Command command)
    {
        if (command.Verb == "y" || command.Verb == "n" ||
            command.Verb == "leave")
        {
            return true;
        }
        Typewriter.TypeLine("Valid commands are: y, n, leave");
        return false;
    }
}