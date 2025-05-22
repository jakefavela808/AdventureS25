namespace AdventureS25;

public static class CommandHandler
{

    public static void Handle(Command command)
    {
        if (States.CurrentStateType == StateTypes.Exploring)
        {
            ExplorationCommandHandler.Handle(command);
        }
        else if (States.CurrentStateType == StateTypes.Fighting)
        {
            CombatCommandHandler.Handle(command);
        }
        else if (States.CurrentStateType == StateTypes.Talking)
        {
            ConversationCommandHandler.Handle(command);
        }
    }
}