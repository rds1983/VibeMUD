namespace VibeMUD.Models;

/// <summary>
/// Represents a single room in the game world
/// </summary>
public class Room
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Terrain { get; set; } = "ground"; // ground, city, cave, forest, etc.

    // Exits: each direction maps to a room reference (areaId, roomId) or null if no exit
    public Dictionary<string, (string areaId, string roomId)?> Exits { get; set; } = new()
    {
        ["north"] = null,
        ["south"] = null,
        ["east"] = null,
        ["west"] = null,
        ["up"] = null,
        ["down"] = null
    };

    // Runtime state
    public List<string> NPCIds { get; set; } = new(); // NPC instances in this room
    public List<Item> Items { get; set; } = new();
    public List<string> PlayerIds { get; set; } = new(); // Player character IDs

    public Room() { }

    public Room(string id, string title, string description)
    {
        Id = id;
        Title = title;
        Description = description;
    }

    public bool HasExit(string direction)
    {
        if (!Exits.ContainsKey(direction))
            return false;

        return Exits[direction] != null;
    }

    public (string areaId, string roomId)? GetExit(string direction)
    {
        if (!Exits.ContainsKey(direction))
            return null;

        return Exits[direction];
    }

    public void AddExit(string direction, string areaId, string roomId)
    {
        if (Exits.ContainsKey(direction))
            Exits[direction] = (areaId, roomId);
        else
            Exits.Add(direction, (areaId, roomId));
    }

    public void RemoveExit(string direction)
    {
        if (Exits.ContainsKey(direction))
            Exits[direction] = null;
    }

    public void AddNPC(string npcId)
    {
        if (!NPCIds.Contains(npcId))
            NPCIds.Add(npcId);
    }

    public void RemoveNPC(string npcId)
    {
        NPCIds.Remove(npcId);
    }

    public void AddItem(Item item)
    {
        Items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        Items.Remove(item);
    }

    public void AddPlayer(string playerId)
    {
        if (!PlayerIds.Contains(playerId))
            PlayerIds.Add(playerId);
    }

    public void RemovePlayer(string playerId)
    {
        PlayerIds.Remove(playerId);
    }

    public List<string> GetOccupants()
    {
        var occupants = new List<string>(NPCIds);
        occupants.AddRange(PlayerIds);
        return occupants;
    }

    public string GetExitDescription()
    {
        var availableExits = new List<string>();
        foreach (var direction in new[] { "north", "south", "east", "west", "up", "down" })
        {
            if (HasExit(direction))
                availableExits.Add(direction);
        }

        return availableExits.Count > 0 ? $"Exits: {string.Join(", ", availableExits)}" : "No visible exits.";
    }
}
