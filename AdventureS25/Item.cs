namespace AdventureS25;

public class Item
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    
    public string InitialDescription { get; private set; }
    public bool IsTakeable { get; private set; }
    
    public bool HasBeenTaken { get; private set; }

    public Item(string name, string description, 
        string initialDescription, bool isTakeable = true)
    {   
        Name = name;
        Description = description;
        InitialDescription = initialDescription;
        IsTakeable = isTakeable;
        ExplorationCommandValidator.AddNoun(name);
    }

    public override string ToString()
    {
        return Name + " - " + Description + " - " + IsTakeable;
    }

    public void Pickup()
    {
        HasBeenTaken = true;
    }

    public string GetLocationDescription()
    {
        if (HasBeenTaken)
        {
            string article = SemanticTools.CreateArticle(Name);
            return "There is " + article + " " + Name + " here.";
        }
        return InitialDescription;
    }
}
