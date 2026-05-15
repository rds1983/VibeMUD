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
        var skillsDir = Path.Combine(_contentPath, "skills");

        // Try to load from skills subdirectory first
        if (Directory.Exists(skillsDir))
        {
            try
            {
                var skillFiles = Directory.GetFiles(skillsDir, "*.json", SearchOption.TopDirectoryOnly);
                foreach (var skillFile in skillFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(skillFile);
                        var skill = JsonSerializer.Deserialize<Skill>(json, JsonOptions);
                        if (skill != null && !string.IsNullOrEmpty(skill.Id))
                            skills[skill.Id] = skill;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to load skill from {Path.GetFileName(skillFile)}", ex);
                    }
                }
                return skills;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load skills from subdirectory", ex);
            }
        }

        // Fall back to single file if subdirectory doesn't exist
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
        var itemsDir = Path.Combine(_contentPath, "items");

        // Try to load from items subdirectory first
        if (Directory.Exists(itemsDir))
        {
            try
            {
                var itemFiles = Directory.GetFiles(itemsDir, "*.json", SearchOption.TopDirectoryOnly);
                foreach (var itemFile in itemFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(itemFile);
                        var item = JsonSerializer.Deserialize<Item>(json, JsonOptions);
                        if (item != null && !string.IsNullOrEmpty(item.Id))
                            items[item.Id] = item;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to load item from {Path.GetFileName(itemFile)}", ex);
                    }
                }
                return items;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load items from subdirectory", ex);
            }
        }

        // Fall back to single file if subdirectory doesn't exist
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
        var areasDir = Path.Combine(_contentPath, "areas");

        // Try to load from areas subdirectory first
        if (Directory.Exists(areasDir))
        {
            try
            {
                var areaFiles = Directory.GetFiles(areasDir, "*.json", SearchOption.TopDirectoryOnly);
                foreach (var areaFile in areaFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(areaFile);
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
                        throw new InvalidOperationException($"Failed to load area from {Path.GetFileName(areaFile)}", ex);
                    }
                }
                return areas;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load areas from subdirectory", ex);
            }
        }

        // Fall back to single file if subdirectory doesn't exist
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
        var npcsDir = Path.Combine(_contentPath, "npcs");

        // Try to load from npcs subdirectory first
        if (Directory.Exists(npcsDir))
        {
            try
            {
                var npcFiles = Directory.GetFiles(npcsDir, "*.json", SearchOption.TopDirectoryOnly);
                foreach (var npcFile in npcFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(npcFile);
                        var npc = JsonSerializer.Deserialize<NPC>(json, JsonOptions);
                        if (npc != null && !string.IsNullOrEmpty(npc.Id))
                            npcs[npc.Id] = npc;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to load NPC from {Path.GetFileName(npcFile)}", ex);
                    }
                }
                return npcs;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load NPCs from subdirectory", ex);
            }
        }

        // Fall back to single file if subdirectory doesn't exist
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

    /* TODO: Implement Potion class
    public Dictionary<string, Potion> LoadPotions(string filename = "potions.json")
    {
        var potions = new Dictionary<string, Potion>();
        var potionsDir = Path.Combine(_contentPath, "potions");

        if (Directory.Exists(potionsDir))
        {
            try
            {
                var potionFiles = Directory.GetFiles(potionsDir, "*.json", SearchOption.TopDirectoryOnly);
                foreach (var potionFile in potionFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(potionFile);
                        var potion = JsonSerializer.Deserialize<Potion>(json, JsonOptions);
                        if (potion != null && !string.IsNullOrEmpty(potion.Id))
                            potions[potion.Id] = potion;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to load potion from {Path.GetFileName(potionFile)}", ex);
                    }
                }
                return potions;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load potions from subdirectory", ex);
            }
        }

        var path = Path.Combine(_contentPath, filename);
        if (!File.Exists(path))
            return potions;

        try
        {
            var json = File.ReadAllText(path);
            var root = JsonDocument.Parse(json).RootElement;

            if (root.TryGetProperty("potions", out var potionsArray))
            {
                foreach (var potionElement in potionsArray.EnumerateArray())
                {
                    var potion = JsonSerializer.Deserialize<Potion>(potionElement.GetRawText(), JsonOptions);
                    if (potion != null && !string.IsNullOrEmpty(potion.Id))
                        potions[potion.Id] = potion;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load potions from {filename}", ex);
        }

        return potions;
    }
    */

    /* TODO: Implement Scroll class
    public Dictionary<string, Scroll> LoadScrolls(string filename = "scrolls.json")
    {
        var scrolls = new Dictionary<string, Scroll>();
        var scrollsDir = Path.Combine(_contentPath, "scrolls");

        if (Directory.Exists(scrollsDir))
        {
            try
            {
                var scrollFiles = Directory.GetFiles(scrollsDir, "*.json", SearchOption.TopDirectoryOnly);
                foreach (var scrollFile in scrollFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(scrollFile);
                        var scroll = JsonSerializer.Deserialize<Scroll>(json, JsonOptions);
                        if (scroll != null && !string.IsNullOrEmpty(scroll.Id))
                            scrolls[scroll.Id] = scroll;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to load scroll from {Path.GetFileName(scrollFile)}", ex);
                    }
                }
                return scrolls;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load scrolls from subdirectory", ex);
            }
        }

        var path = Path.Combine(_contentPath, filename);
        if (!File.Exists(path))
            return scrolls;

        try
        {
            var json = File.ReadAllText(path);
            var root = JsonDocument.Parse(json).RootElement;

            if (root.TryGetProperty("scrolls", out var scrollsArray))
            {
                foreach (var scrollElement in scrollsArray.EnumerateArray())
                {
                    var scroll = JsonSerializer.Deserialize<Scroll>(scrollElement.GetRawText(), JsonOptions);
                    if (scroll != null && !string.IsNullOrEmpty(scroll.Id))
                        scrolls[scroll.Id] = scroll;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load scrolls from {filename}", ex);
        }

        return scrolls;
    }
    */

    /* TODO: Implement Shop class
    public Dictionary<string, Shop> LoadShops(string filename = "shops.json")
    {
        var shops = new Dictionary<string, Shop>();
        var shopsDir = Path.Combine(_contentPath, "shops");

        if (Directory.Exists(shopsDir))
        {
            try
            {
                var shopFiles = Directory.GetFiles(shopsDir, "*.json", SearchOption.TopDirectoryOnly);
                foreach (var shopFile in shopFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(shopFile);
                        var shop = JsonSerializer.Deserialize<Shop>(json, JsonOptions);
                        if (shop != null && !string.IsNullOrEmpty(shop.Id))
                            shops[shop.Id] = shop;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to load shop from {Path.GetFileName(shopFile)}", ex);
                    }
                }
                return shops;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load shops from subdirectory", ex);
            }
        }

        var path = Path.Combine(_contentPath, filename);
        if (!File.Exists(path))
            return shops;

        try
        {
            var json = File.ReadAllText(path);
            var root = JsonDocument.Parse(json).RootElement;

            if (root.TryGetProperty("shops", out var shopsArray))
            {
                foreach (var shopElement in shopsArray.EnumerateArray())
                {
                    var shop = JsonSerializer.Deserialize<Shop>(shopElement.GetRawText(), JsonOptions);
                    if (shop != null && !string.IsNullOrEmpty(shop.Id))
                        shops[shop.Id] = shop;
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load shops from {filename}", ex);
        }

        return shops;
    }
    */

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
