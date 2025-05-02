namespace AdventureS25;

using AdventureS25;

public static class ExplorationCommandValidator
{
    public static List<string> Verbs = new List<string>
        {"go", "eat", "take", "drop", "drink", "read"};
    
    public static List<string> StandaloneVerbs = new List<string>
    {
        "exit", "inventory", "look", "tron", "troff",
        "nouns", "verbs", "fight", "explore", "talk", "beerme", 
        "unbeerme", "puke", "tidyup", "teleport", "connect", "disconnect", "pals"
    };
    
    public static List<string> Nouns = new List<string>
    {
        "bagel", "apple", "beer", "east", "west", "north", "south",
        "up", "down", "sword", "note"
    };
    
    public static bool IsValid(Command command)
    {
        bool isValid = false;
        
        if (IsVerb(command.Verb))
        {
            Debugger.Write("Valid verb: " + command.Verb);
            
            if (IsStandaloneVerb(command.Verb))
            {
                Debugger.Write("Valid standalone verb: " + command.Verb);

                if (HasNoNoun(command))
                {
                    isValid = true;
                }
                else
                {
                    Typewriter.TypeLine("I don't know how to do that.");
                    Console.Clear();
                    Player.Look();
                }
            }
            else if (IsNoun(command.Noun))
            {
                Debugger.Write("Valid Noun: " + command.Noun);
                isValid = true;
            }
            else
            {
                Typewriter.TypeLine("I don't know how to do that.");
                Console.Clear();
                Player.Look();
            }
        }
        else
        {
            Typewriter.TypeLine("I don't know the word " + command.Verb + ".");
            Console.Clear();
            Player.Look();
        }
            
        return isValid;
    }

    private static bool HasNoNoun(Command command)
    {
        if (command.Noun == String.Empty)
        {
            return true;
        }
        return false;
    }

    private static bool IsNoun(string commandNoun)
    {
        if (Nouns.Contains(commandNoun))
        {
            return true;
        }
        return false;
    }

    private static bool IsStandaloneVerb(string commandVerb)
    {
        if (StandaloneVerbs.Contains(commandVerb))
        {
            return true;
        }
        return false;
    }

    private static bool IsVerb(string commandVerb)
    {
        if (Verbs.Contains(commandVerb) || StandaloneVerbs.Contains(commandVerb))
        {
            return true;
        }
        
        return false;
    }

    public static void AddNoun(string noun)
    {
        noun = noun.ToLower();
        if (!Nouns.Contains(noun))
        {
            Nouns.Add(noun);
        }
    }

    public static List<string> GetNouns()
    {
        return Nouns;
    }

    public static List<string> GetVerbs()
    {
        return Verbs.Concat(StandaloneVerbs).ToList();
    }
}