namespace AdventureS25;

using AdventureS25;

public static class States
{
    public static State CurrentState { get; set; }
    
    public static StateTypes CurrentStateType { get; set; }
    
    private static Dictionary<StateTypes, State> states = 
        new Dictionary<StateTypes, State>();

    public static void Initialize()
    {
        Add(StateTypes.Exploring);
        Add(StateTypes.Fighting);
        Add(StateTypes.Talking);
        
        ChangeState(StateTypes.Exploring);
    }
    
    public static void Add(StateTypes stateType)
    {
        State newState = new State(stateType);
        states.Add(stateType, newState);
    }

    public static void ChangeState(StateTypes stateType)
    {
        Console.Clear();
        if (!states.ContainsKey(stateType))
        {
            return;
        }
        CurrentState = states[stateType];
        CurrentStateType = stateType;
        
        Typewriter.TypeLine("Changing to state " + stateType);
    }
}