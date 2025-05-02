namespace AdventureS25;

using AdventureS25;

public static class ConversationCommandValidator
{
    public static bool IsValid(Command command)
    {
        // Check if we're awaiting starter selection
        var awaitingStarterSelection = ConversationCommandHandler.IsAwaitingStarterSelection();
        if (awaitingStarterSelection)
        {
            if (command.Verb == "1" || command.Verb == "2" || command.Verb == "3")
            {
                return true;
            }
            Typewriter.TypeLine("Valid commands are: 1, 2, 3");
            return false;
        }
        else
        {
            if (command.Verb == "yes" || command.Verb == "no" || command.Verb == "leave")
            {
                return true;
            }
            Typewriter.TypeLine("Valid commands are: yes, no, leave");
            return false;
        }
    }
}