using System.Text.Json;

namespace AdventureS25;

public static class Items
{
    private static Dictionary<string, Item> nameToItem = 
        new Dictionary<string, Item>(StringComparer.OrdinalIgnoreCase);
    
    public static void Initialize()
    {
        string path = Path.Combine(Environment.CurrentDirectory, "Items.json");
        string rawText = File.ReadAllText(path);
        
        ItemsJsonData? data = JsonSerializer.Deserialize<ItemsJsonData>(rawText);

        if (data == null || data.Items == null)
        {
            Console.WriteLine("[ERROR] Items.json could not be loaded or is empty.");
            return;
        }

        foreach (ItemJsonData item in data.Items)
        {
            string itemName = item.Name ?? "Unnamed Item";
            string itemDescription = item.Description ?? "No description.";
            string itemInitialDescription = item.InitialDescription ?? "It's an item.";

            Item newItem = CreateItem(itemName, itemDescription,
                itemInitialDescription, item.IsTakeable);
            
            if (!string.IsNullOrEmpty(item.Location))
            {
                Map.AddItem(newItem.Name, item.Location); 
            }
        }
    }

    public static Item CreateItem(string name, string description,
        string initialDescription, bool isTakeable)
    {
        Item newItem = new Item(name,
            description, 
            initialDescription, isTakeable);
        nameToItem[name.ToLower()] = newItem;
        return newItem;
    }

    public static Item? GetItemByName(string itemName)
    {
        itemName = itemName.ToLower();
        if (nameToItem.ContainsKey(itemName))
        {
            return nameToItem[itemName];
        }
        return null;
    }
}