namespace VibeMUD.Models;

/// <summary>
/// Represents a geographic area in the game world
/// </summary>
public class Area
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Level Range
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }

    // Rooms
    public Dictionary<string, Room> Rooms { get; set; } = new();

    // Spawn Point
    public string? SpawnRoomId { get; set; }

    // Shop
    public bool HasShop { get; set; }
    public string? ShopId { get; set; }

    public Area() { }

    public Area(string id, string name, string description, int minLevel, int maxLevel)
    {
        Id = id;
        Name = name;
        Description = description;
        MinLevel = minLevel;
        MaxLevel = maxLevel;
    }

    public void AddRoom(Room room)
    {
        if (!Rooms.ContainsKey(room.Id))
            Rooms.Add(room.Id, room);
    }

    public Room? GetRoom(string roomId)
    {
        if (Rooms.TryGetValue(roomId, out var room))
            return room;

        return null;
    }

    public bool RoomExists(string roomId)
    {
        return Rooms.ContainsKey(roomId);
    }

    public int GetRoomCount()
    {
        return Rooms.Count;
    }

    public bool IsValidForLevel(int playerLevel)
    {
        return playerLevel >= MinLevel && playerLevel <= MaxLevel;
    }

    public string GetLevelRangeString()
    {
        return $"{MinLevel}-{MaxLevel}";
    }
}
