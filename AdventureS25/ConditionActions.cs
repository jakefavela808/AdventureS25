namespace AdventureS25;

using AdventureS25;

public static class ConditionActions
{
    public static Action WriteOutput(string message)
    {
        return () => { Typewriter.TypeLine(message); };
    }
    
    // add to inventory
    public static Action AddItemToInventory(string itemName)
    {
        return () => { Player.AddItemToInventory(itemName); };
    }
    
    // remove from inventory
    public static Action RemoveItemFromInventory(string itemName)
    {
        return () => Player.RemoveItemFromInventory(itemName); 
    }
    
    // add object to location
    public static Action AddItemToLocation(string itemName, string locationName)
    {
        return () => Map.AddItem(itemName, locationName);
    }
    
    // remove object from location
    public static Action RemoveItemFromLocation(string itemName, string locationName)
    {
        return () => Map.RemoveItem(itemName, locationName);
    }
    
    // move player to location
    public static Action MovePlayerToLocation(string locationName)
    {
        return () => Player.MoveToLocation(locationName);
    }
    
    // create a map connection
    public static Action AddMapConnection(string startLocationName, string direction,
        string endLocationName)
    {
        return () => Map.AddConnection(startLocationName, direction, endLocationName);
    }
    
    // remove a map connection
    public static Action RemoveMapConnection(string startLocationName, string direction)
    {
        return () => Map.RemoveConnection(startLocationName, direction);
    }
}