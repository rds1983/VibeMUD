namespace VibeMUD.Core;

using VibeMUD.Models;

/// <summary>
/// Represents the entire game world state
/// </summary>
public class GameState
{
    public Dictionary<string, Character> Characters { get; set; } = new();
    public Dictionary<string, NPC> NPCs { get; set; } = new();
    public Dictionary<string, Area> Areas { get; set; } = new();
    public Dictionary<string, Item> Items { get; set; } = new();
    public Dictionary<string, Skill> Skills { get; set; } = new();

    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public bool IsRunning { get; set; }

    /// <summary>
    /// Gets a room by area and room ID
    /// </summary>
    public Room? GetRoom(string areaId, string roomId)
    {
        if (!Areas.TryGetValue(areaId, out var area))
            return null;

        return area.GetRoom(roomId);
    }

    /// <summary>
    /// Gets a character by ID
    /// </summary>
    public Character? GetCharacter(string characterId)
    {
        if (Characters.TryGetValue(characterId, out var character))
            return character;

        return null;
    }

    /// <summary>
    /// Gets an NPC by ID
    /// </summary>
    public NPC? GetNPC(string npcId)
    {
        if (NPCs.TryGetValue(npcId, out var npc))
            return npc;

        return null;
    }

    /// <summary>
    /// Adds a character to the game
    /// </summary>
    public void AddCharacter(Character character)
    {
        Characters[character.Id] = character;
    }

    /// <summary>
    /// Removes a character from the game
    /// </summary>
    public void RemoveCharacter(string characterId)
    {
        if (Characters.TryGetValue(characterId, out var character))
        {
            // Remove from room if present
            if (!string.IsNullOrEmpty(character.CurrentAreaId) && !string.IsNullOrEmpty(character.CurrentRoomId))
            {
                var room = GetRoom(character.CurrentAreaId, character.CurrentRoomId);
                room?.RemovePlayer(characterId);
            }

            Characters.Remove(characterId);
        }
    }

    /// <summary>
    /// Spawns an NPC instance in a room
    /// </summary>
    public NPC? SpawnNPC(string npcTemplateId, string areaId, string roomId)
    {
        if (!NPCs.TryGetValue(npcTemplateId, out var template))
            return null;

        var npc = new NPC(template.Id, template.Name, template.Description, template.Level)
        {
            Health = template.MaxHealth,
            MaxHealth = template.MaxHealth,
            Mana = template.MaxMana,
            MaxMana = template.MaxMana,
            Strength = template.Strength,
            Dexterity = template.Dexterity,
            Constitution = template.Constitution,
            Intelligence = template.Intelligence,
            Wisdom = template.Wisdom,
            Charisma = template.Charisma,
            Behavior = template.Behavior,
            PatrolPath = new List<string>(template.PatrolPath),
            SkillIds = new List<string>(template.SkillIds),
            SpecialAttackIds = new List<string>(template.SpecialAttackIds),
            Flags = new List<string>(template.Flags),
            GoldMin = template.GoldMin,
            GoldMax = template.GoldMax,
            LootTable = new Dictionary<string, double>(template.LootTable),
            Resistances = new Dictionary<string, double>(template.Resistances),
            DamageTypes = new List<string>(template.DamageTypes),
            RespawnTimeSeconds = template.RespawnTimeSeconds,
            CurrentAreaId = areaId,
            CurrentRoomId = roomId
        };

        var room = GetRoom(areaId, roomId);
        room?.AddNPC(npc.Id);

        return npc;
    }

    /// <summary>
    /// Gets all NPCs in a room
    /// </summary>
    public List<NPC> GetNPCsInRoom(string areaId, string roomId)
    {
        var room = GetRoom(areaId, roomId);
        if (room == null)
            return new List<NPC>();

        var npcsInRoom = new List<NPC>();
        foreach (var npcId in room.NPCIds)
        {
            if (NPCs.TryGetValue(npcId, out var npc))
                npcsInRoom.Add(npc);
        }

        return npcsInRoom;
    }

    /// <summary>
    /// Gets all players in a room
    /// </summary>
    public List<Character> GetPlayersInRoom(string areaId, string roomId)
    {
        var room = GetRoom(areaId, roomId);
        if (room == null)
            return new List<Character>();

        var playersInRoom = new List<Character>();
        foreach (var playerId in room.PlayerIds)
        {
            if (Characters.TryGetValue(playerId, out var character))
                playersInRoom.Add(character);
        }

        return playersInRoom;
    }

    /// <summary>
    /// Gets items in a room
    /// </summary>
    public List<Item> GetItemsInRoom(string areaId, string roomId)
    {
        var room = GetRoom(areaId, roomId);
        return room?.Items ?? new List<Item>();
    }
}
