namespace VibeMUD.Systems;

using VibeMUD.Models;

/// <summary>
/// Manages NPC AI behaviors including wandering, patrolling, and aggressive behaviors
/// </summary>
public class NPCBehaviorSystem
{
    public enum BehaviorType
    {
        Wander,
        Patrol,
        Aggressive
    }

    public class BehaviorAction
    {
        public string ActionType { get; set; } = string.Empty; // "move", "attack", "none"
        public string? TargetRoomId { get; set; }
        public string? TargetCharacterId { get; set; }
        public string? Direction { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    private static readonly Random Random = new();
    private static readonly string[] Directions = { "north", "south", "east", "west", "up", "down" };

    /// <summary>
    /// Determines the next action for an NPC based on its behavior type
    /// </summary>
    public static BehaviorAction DetermineBehaviorAction(
        NPC npc,
        Room currentRoom,
        List<Character> playersInRoom,
        Dictionary<string, Room>? availableRooms = null)
    {
        var behavior = npc.Behavior.ToLower();

        return behavior switch
        {
            "aggressive" => DetermineAggressiveBehavior(npc, currentRoom, playersInRoom),
            "patrol" => DeterminePatrolBehavior(npc, currentRoom, availableRooms),
            "wander" => DetermineWanderBehavior(npc, currentRoom, availableRooms),
            _ => new BehaviorAction { ActionType = "none", Message = $"The {npc.Name} stands idle." }
        };
    }

    /// <summary>
    /// Aggressive behavior: attack players on sight or move toward them
    /// </summary>
    private static BehaviorAction DetermineAggressiveBehavior(NPC npc, Room currentRoom, List<Character> playersInRoom)
    {
        var action = new BehaviorAction();

        // If there are players in the room, engage them
        if (playersInRoom.Count > 0)
        {
            var target = playersInRoom[Random.Next(playersInRoom.Count)];
            action.ActionType = "attack";
            action.TargetCharacterId = target.Id;
            action.Message = $"The {npc.Name} attacks {target.Name}!";
            return action;
        }

        // If no players, move to adjacent room looking for prey
        var validExits = currentRoom.Exits
            .Where(exit => exit.Value.HasValue)
            .ToList();

        if (validExits.Count == 0)
        {
            action.ActionType = "none";
            action.Message = $"The {npc.Name} prowls restlessly.";
            return action;
        }

        var randomExit = validExits[Random.Next(validExits.Count)];
        action.ActionType = "move";
        action.Direction = randomExit.Key;
        action.TargetRoomId = randomExit.Value!.Value.roomId;
        action.Message = $"The {npc.Name} moves {randomExit.Key}.";
        return action;
    }

    /// <summary>
    /// Patrol behavior: follow a predefined path through rooms
    /// </summary>
    private static BehaviorAction DeterminePatrolBehavior(NPC npc, Room currentRoom, Dictionary<string, Room>? availableRooms)
    {
        var action = new BehaviorAction();

        // No patrol path defined, default to idle
        if (npc.PatrolPath == null || npc.PatrolPath.Count == 0)
        {
            action.ActionType = "none";
            action.Message = $"The {npc.Name} stands watch.";
            return action;
        }

        // Find current position in patrol path
        int currentIndex = npc.PatrolPath.IndexOf(currentRoom.Id);

        if (currentIndex < 0)
        {
            // Not on patrol path, move to the start
            var firstRoomId = npc.PatrolPath[0];
            action.ActionType = "teleport";
            action.TargetRoomId = firstRoomId;
            action.Message = $"The {npc.Name} returns to patrol.";
            return action;
        }

        // Move to next room in patrol path (circular)
        int nextIndex = (currentIndex + 1) % npc.PatrolPath.Count;
        var nextRoomId = npc.PatrolPath[nextIndex];

        // Find direction to next room
        var exitDirection = FindDirectionToRoom(currentRoom, nextRoomId, availableRooms);

        action.ActionType = "move";
        action.TargetRoomId = nextRoomId;
        action.Direction = exitDirection ?? "unknown";
        action.Message = $"The {npc.Name} patrols {(exitDirection ?? "onward")}.";
        return action;
    }

    /// <summary>
    /// Wander behavior: randomly move between adjacent rooms
    /// </summary>
    private static BehaviorAction DetermineWanderBehavior(NPC npc, Room currentRoom, Dictionary<string, Room>? availableRooms)
    {
        var action = new BehaviorAction();

        // Get valid exits
        var validExits = currentRoom.Exits
            .Where(exit => exit.Value.HasValue)
            .ToList();

        // Occasionally stay still (30% chance)
        if (Random.Next(100) < 30)
        {
            action.ActionType = "none";
            action.Message = $"The {npc.Name} wanders aimlessly.";
            return action;
        }

        // If no valid exits, stay put
        if (validExits.Count == 0)
        {
            action.ActionType = "none";
            action.Message = $"The {npc.Name} looks around.";
            return action;
        }

        // Move to random adjacent room
        var randomExit = validExits[Random.Next(validExits.Count)];
        action.ActionType = "move";
        action.Direction = randomExit.Key;
        action.TargetRoomId = randomExit.Value!.Value.roomId;
        action.Message = $"The {npc.Name} wanders {randomExit.Key}.";
        return action;
    }

    /// <summary>
    /// Finds the direction from current room to target room
    /// </summary>
    private static string? FindDirectionToRoom(Room currentRoom, string targetRoomId, Dictionary<string, Room>? availableRooms)
    {
        foreach (var (direction, exit) in currentRoom.Exits)
        {
            if (exit.HasValue && exit.Value.roomId == targetRoomId)
                return direction;
        }

        // If not directly connected, search through available rooms
        if (availableRooms != null)
        {
            var visited = new HashSet<string> { currentRoom.Id };
            var queue = new Queue<(string roomId, string direction)>();

            foreach (var (direction, exit) in currentRoom.Exits)
            {
                if (exit.HasValue)
                {
                    queue.Enqueue((exit.Value.roomId, direction));
                }
            }

            while (queue.Count > 0)
            {
                var (roomId, firstDirection) = queue.Dequeue();

                if (roomId == targetRoomId)
                    return firstDirection;

                if (!visited.Add(roomId))
                    continue;

                if (availableRooms.TryGetValue(roomId, out var room))
                {
                    foreach (var (direction, exit) in room.Exits)
                    {
                        if (exit.HasValue && !visited.Contains(exit.Value.roomId))
                        {
                            queue.Enqueue((exit.Value.roomId, firstDirection));
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Determines if an NPC should respawn after being defeated
    /// </summary>
    public static bool ShouldRespawn(NPC npc)
    {
        if (npc.IsAlive())
            return false;

        if (npc.LastRespawnTime == null)
            return true;

        var timeSinceRespawn = DateTime.UtcNow - npc.LastRespawnTime.Value;
        return timeSinceRespawn.TotalSeconds >= npc.RespawnTimeSeconds;
    }

    /// <summary>
    /// Respawns an NPC with full health and mana
    /// </summary>
    public static void Respawn(NPC npc)
    {
        npc.RestoreCombat();
        npc.LastRespawnTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates NPC behavior configuration
    /// </summary>
    public static bool ValidateBehavior(NPC npc)
    {
        var behavior = npc.Behavior.ToLower();

        // Check valid behavior type
        if (!new[] { "wander", "patrol", "aggressive" }.Contains(behavior))
            return false;

        // Check patrol path if patrol behavior
        if (behavior == "patrol" && (npc.PatrolPath == null || npc.PatrolPath.Count == 0))
            return false;

        return true;
    }
}
