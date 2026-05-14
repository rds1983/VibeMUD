namespace VibeMUD.Data;

using System.Text.Json;
using VibeMUD.Models;

/// <summary>
/// Loads game content from JSON files
/// </summary>
public class JsonDataLoader
{
    private readonly string _contentPath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public JsonDataLoader(string contentPath)
    {
        _contentPath = contentPath;
    }

    public Dictionary<string, Skill> LoadSkills(string filename = "skills.json")
    {
        var skills = new Dictionary<string, Skill>();
        var path = Path.Combine(_contentPath, filename);

        if (!File.Exists(path))
            return skills;

        try
        {
            var json = File.ReadAllText(path);
            var root = JsonDocument.Parse(json).RootElement;

            if (root.TryGetProperty("skills", out var skillsArray))
            {
                foreach (var skillElement in skillsArray.EnumerateArray())
                {
                    var skill = JsonSerializer.Deserialize<Skill>(skillElement.GetRawText(), JsonOptions);
                    if (skill != null && !string.IsNullOrEmpty(skill.Id))
                        skills[skill.Id] = skill;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load skills from {filename}", ex);
        }

        return skills;
    }

    public Dictionary<string, Item> LoadItems(string filename = "items.json")
    {
        var items = new Dictionary<string, Item>();
        var path = Path.Combine(_contentPath, filename);

        if (!File.Exists(path))
            return items;

        try
        {
            var json = File.ReadAllText(path);
            var root = JsonDocument.Parse(json).RootElement;

            if (root.TryGetProperty("items", out var itemsArray))
            {
                foreach (var itemElement in itemsArray.EnumerateArray())
                {
                    var item = JsonSerializer.Deserialize<Item>(itemElement.GetRawText(), JsonOptions);
                    if (item != null && !string.IsNullOrEmpty(item.Id))
                        items[item.Id] = item;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load items from {filename}", ex);
        }

        return items;
    }

    public Dictionary<string, Area> LoadAreas(string filename = "areas.json")
    {
        var areas = new Dictionary<string, Area>();
        var path = Path.Combine(_contentPath, filename);

        if (!File.Exists(path))
            return areas;

        try
        {
            var json = File.ReadAllText(path);
            var root = JsonDocument.Parse(json).RootElement;

            if (root.TryGetProperty("areas", out var areasArray))
            {
                foreach (var areaElement in areasArray.EnumerateArray())
                {
                    var area = ParseArea(areaElement);
                    if (area != null && !string.IsNullOrEmpty(area.Id))
                        areas[area.Id] = area;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load areas from {filename}", ex);
        }

        return areas;
    }

    private Area? ParseArea(JsonElement areaElement)
    {
        var area = new Area();

        if (areaElement.TryGetProperty("id", out var id))
            area.Id = id.GetString() ?? string.Empty;

        if (areaElement.TryGetProperty("name", out var name))
            area.Name = name.GetString() ?? string.Empty;

        if (areaElement.TryGetProperty("description", out var description))
            area.Description = description.GetString() ?? string.Empty;

        if (areaElement.TryGetProperty("levelRange", out var levelRange))
        {
            if (levelRange.TryGetProperty("min", out var min))
                area.MinLevel = min.GetInt32();
            if (levelRange.TryGetProperty("max", out var max))
                area.MaxLevel = max.GetInt32();
        }

        if (areaElement.TryGetProperty("spawnPoint", out var spawnPoint) && spawnPoint.ValueKind != JsonValueKind.Null)
        {
            if (spawnPoint.TryGetProperty("roomId", out var roomId))
                area.SpawnRoomId = roomId.GetString();
        }

        if (areaElement.TryGetProperty("hasShop", out var hasShop))
            area.HasShop = hasShop.GetBoolean();

        if (areaElement.TryGetProperty("shopId", out var shopId))
            area.ShopId = shopId.GetString();

        if (areaElement.TryGetProperty("rooms", out var roomsArray))
        {
            foreach (var roomElement in roomsArray.EnumerateArray())
            {
                var room = ParseRoom(roomElement);
                if (room != null)
                    area.AddRoom(room);
            }
        }

        return area;
    }

    private Room? ParseRoom(JsonElement roomElement)
    {
        var room = new Room();

        if (roomElement.TryGetProperty("id", out var id))
            room.Id = id.GetString() ?? string.Empty;

        if (roomElement.TryGetProperty("title", out var title))
            room.Title = title.GetString() ?? string.Empty;

        if (roomElement.TryGetProperty("description", out var description))
            room.Description = description.GetString() ?? string.Empty;

        if (roomElement.TryGetProperty("terrain", out var terrain))
            room.Terrain = terrain.GetString() ?? "ground";

        if (roomElement.TryGetProperty("exits", out var exitsObj))
        {
            foreach (var direction in new[] { "north", "south", "east", "west", "up", "down" })
            {
                if (exitsObj.TryGetProperty(direction, out var exitValue))
                {
                    if (exitValue.ValueKind == JsonValueKind.Null)
                    {
                        room.Exits[direction] = null;
                    }
                    else
                    {
                        string? areaId = null;
                        string? roomId = null;

                        if (exitValue.TryGetProperty("areaId", out var areaIdElem))
                            areaId = areaIdElem.GetString();

                        if (exitValue.TryGetProperty("roomId", out var roomIdElem))
                            roomId = roomIdElem.GetString();

                        if (!string.IsNullOrEmpty(areaId) && !string.IsNullOrEmpty(roomId))
                            room.Exits[direction] = (areaId, roomId);
                    }
                }
            }
        }

        if (roomElement.TryGetProperty("npcs", out var npcsArray))
        {
            foreach (var npc in npcsArray.EnumerateArray())
            {
                if (npc.ValueKind == JsonValueKind.String)
                    room.NPCIds.Add(npc.GetString() ?? string.Empty);
            }
        }

        return room;
    }

    public Dictionary<string, NPC> LoadNPCs(string filename = "npcs.json")
    {
        var npcs = new Dictionary<string, NPC>();
        var path = Path.Combine(_contentPath, filename);

        if (!File.Exists(path))
            return npcs;

        try
        {
            var json = File.ReadAllText(path);
            var root = JsonDocument.Parse(json).RootElement;

            if (root.TryGetProperty("npcs", out var npcsArray))
            {
                foreach (var npcElement in npcsArray.EnumerateArray())
                {
                    var npc = ParseNPC(npcElement);
                    if (npc != null && !string.IsNullOrEmpty(npc.Id))
                        npcs[npc.Id] = npc;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load NPCs from {filename}", ex);
        }

        return npcs;
    }

    private NPC? ParseNPC(JsonElement npcElement)
    {
        var npc = JsonSerializer.Deserialize<NPC>(npcElement.GetRawText(), JsonOptions);
        return npc;
    }

    public bool ValidateJsonFile(string filename)
    {
        var path = Path.Combine(_contentPath, filename);

        if (!File.Exists(path))
            return false;

        try
        {
            var json = File.ReadAllText(path);
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
