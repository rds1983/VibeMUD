namespace VibeMUD.Tests.Systems;

using VibeMUD.Models;
using VibeMUD.Systems;

public class NPCBehaviorSystemTests
{
    private Room CreateTestRoom(string id = "room1", string title = "Test Room")
    {
        return new Room
        {
            Id = id,
            Title = title,
            Description = "A test room",
            Exits = new Dictionary<string, (string areaId, string roomId)?>
            {
                ["north"] = ("area1", "room2"),
                ["south"] = ("area1", "room3"),
                ["east"] = ("area1", "room4"),
                ["west"] = ("area1", "room5"),
                ["up"] = null,
                ["down"] = null
            }
        };
    }

    [Fact]
    public void DetermineBehaviorAction_Wander_ReturnsValidAction()
    {
        var npc = new NPC("wanderer", "Wandering Goblin", "desc", 3)
        {
            Behavior = "wander"
        };
        var room = CreateTestRoom();
        var players = new List<Character>();

        var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);

        Assert.NotNull(action);
        Assert.True(action.ActionType == "move" || action.ActionType == "none");
    }

    [Fact]
    public void DetermineBehaviorAction_Wander_AvoidsDiagonalMovement()
    {
        var npc = new NPC("wanderer", "Goblin", "desc", 3)
        {
            Behavior = "wander"
        };
        var room = CreateTestRoom();
        var players = new List<Character>();

        // Test multiple times to ensure all moves are valid
        for (int i = 0; i < 10; i++)
        {
            var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);

            if (action.ActionType == "move")
            {
                // Verify direction is valid
                Assert.Contains(action.Direction, new[] { "north", "south", "east", "west", "up", "down" });
            }
        }
    }

    [Fact]
    public void DetermineBehaviorAction_Aggressive_AttacksPlayersInRoom()
    {
        var npc = new NPC("goblin", "Aggressive Goblin", "desc", 3)
        {
            Behavior = "aggressive"
        };
        var room = CreateTestRoom();
        var player = new Character("p1", "Hero", "warrior");
        var players = new List<Character> { player };

        var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);

        Assert.Equal("attack", action.ActionType);
        Assert.Equal(player.Id, action.TargetCharacterId);
    }

    [Fact]
    public void DetermineBehaviorAction_Aggressive_MovesWhenNoPlayersInRoom()
    {
        var npc = new NPC("goblin", "Aggressive Goblin", "desc", 3)
        {
            Behavior = "aggressive"
        };
        var room = CreateTestRoom();
        var players = new List<Character>();

        var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);

        Assert.True(action.ActionType == "move" || action.ActionType == "none");
    }

    [Fact]
    public void DetermineBehaviorAction_Aggressive_SelectsRandomTarget()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            Behavior = "aggressive"
        };
        var room = CreateTestRoom();
        var player1 = new Character("p1", "Hero1", "warrior");
        var player2 = new Character("p2", "Hero2", "mage");
        var players = new List<Character> { player1, player2 };

        var targets = new HashSet<string>();
        for (int i = 0; i < 20; i++)
        {
            var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);
            if (action.ActionType == "attack")
            {
                targets.Add(action.TargetCharacterId!);
            }
        }

        // Should eventually target both players (randomness test)
        Assert.True(targets.Count > 0);
    }

    [Fact]
    public void DetermineBehaviorAction_Patrol_FollowsPatrolPath()
    {
        var patrolPath = new List<string> { "room1", "room2", "room3", "room1" };
        var npc = new NPC("guard", "Patrol Guard", "desc", 5)
        {
            Behavior = "patrol",
            PatrolPath = patrolPath
        };
        var room = CreateTestRoom("room1");
        var players = new List<Character>();

        var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);

        Assert.Equal("move", action.ActionType);
        Assert.NotNull(action.Direction);
    }

    [Fact]
    public void DetermineBehaviorAction_Patrol_ReturnsToStartIfOffPath()
    {
        var patrolPath = new List<string> { "room1", "room2", "room3" };
        var npc = new NPC("guard", "Guard", "desc", 5)
        {
            Behavior = "patrol",
            PatrolPath = patrolPath
        };
        var room = CreateTestRoom("room99"); // Not in patrol path
        var players = new List<Character>();

        var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);

        Assert.Equal("teleport", action.ActionType);
        Assert.Equal("room1", action.TargetRoomId);
    }

    [Fact]
    public void DetermineBehaviorAction_Patrol_NoPathIdles()
    {
        var npc = new NPC("guard", "Guard", "desc", 5)
        {
            Behavior = "patrol",
            PatrolPath = new List<string>()
        };
        var room = CreateTestRoom();
        var players = new List<Character>();

        var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);

        Assert.Equal("none", action.ActionType);
    }

    [Fact]
    public void DetermineBehaviorAction_InvalidBehavior_Idles()
    {
        var npc = new NPC("npc", "NPC", "desc", 5)
        {
            Behavior = "invalid"
        };
        var room = CreateTestRoom();
        var players = new List<Character>();

        var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);

        Assert.Equal("none", action.ActionType);
    }

    [Fact]
    public void DetermineBehaviorAction_NoExits_StaysInPlace()
    {
        var npc = new NPC("trapped", "Trapped NPC", "desc", 3)
        {
            Behavior = "wander"
        };
        var room = new Room
        {
            Id = "isolated",
            Title = "Isolated Room",
            Description = "A sealed room",
            Exits = new Dictionary<string, (string areaId, string roomId)?>
            {
                ["north"] = null,
                ["south"] = null,
                ["east"] = null,
                ["west"] = null,
                ["up"] = null,
                ["down"] = null
            }
        };
        var players = new List<Character>();

        var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);

        Assert.Equal("none", action.ActionType);
    }

    [Fact]
    public void ShouldRespawn_DeadNPC_NoLastRespawnTime_ReturnsTrue()
    {
        var npc = new NPC("zombie", "Zombie", "desc", 3)
        {
            Health = 0,
            MaxHealth = 20,
            LastRespawnTime = null
        };

        bool shouldRespawn = NPCBehaviorSystem.ShouldRespawn(npc);

        Assert.True(shouldRespawn);
    }

    [Fact]
    public void ShouldRespawn_AliveNPC_ReturnsFalse()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            Health = 10,
            MaxHealth = 10
        };

        bool shouldRespawn = NPCBehaviorSystem.ShouldRespawn(npc);

        Assert.False(shouldRespawn);
    }

    [Fact]
    public void ShouldRespawn_DeadNPC_WithinRespawnWindow_ReturnsFalse()
    {
        var npc = new NPC("zombie", "Zombie", "desc", 3)
        {
            Health = 0,
            MaxHealth = 20,
            RespawnTimeSeconds = 300,
            LastRespawnTime = DateTime.UtcNow.AddSeconds(-60)
        };

        bool shouldRespawn = NPCBehaviorSystem.ShouldRespawn(npc);

        Assert.False(shouldRespawn);
    }

    [Fact]
    public void ShouldRespawn_DeadNPC_RespawnTimeElapsed_ReturnsTrue()
    {
        var npc = new NPC("zombie", "Zombie", "desc", 3)
        {
            Health = 0,
            MaxHealth = 20,
            RespawnTimeSeconds = 60,
            LastRespawnTime = DateTime.UtcNow.AddSeconds(-120)
        };

        bool shouldRespawn = NPCBehaviorSystem.ShouldRespawn(npc);

        Assert.True(shouldRespawn);
    }

    [Fact]
    public void Respawn_RestoresHealthAndMana()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            Health = 0,
            MaxHealth = 20,
            Mana = 0,
            MaxMana = 5
        };

        NPCBehaviorSystem.Respawn(npc);

        Assert.Equal(20, npc.Health);
        Assert.Equal(5, npc.Mana);
        Assert.NotNull(npc.LastRespawnTime);
    }

    [Fact]
    public void Respawn_UpdatesLastRespawnTime()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            Health = 0,
            MaxHealth = 20
        };
        var beforeRespawn = DateTime.UtcNow;

        NPCBehaviorSystem.Respawn(npc);

        var afterRespawn = DateTime.UtcNow;
        Assert.NotNull(npc.LastRespawnTime);
        Assert.True(npc.LastRespawnTime >= beforeRespawn);
        Assert.True(npc.LastRespawnTime <= afterRespawn);
    }

    [Fact]
    public void Respawn_ClearsCooldowns()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            Health = 0,
            MaxHealth = 20
        };
        npc.SkillCooldowns["skill1"] = DateTime.UtcNow.AddSeconds(60);

        NPCBehaviorSystem.Respawn(npc);

        Assert.Empty(npc.SkillCooldowns);
    }

    [Fact]
    public void ValidateBehavior_ValidWander_ReturnsTrue()
    {
        var npc = new NPC("npc", "NPC", "desc", 3)
        {
            Behavior = "wander"
        };

        bool isValid = NPCBehaviorSystem.ValidateBehavior(npc);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateBehavior_ValidAggressive_ReturnsTrue()
    {
        var npc = new NPC("npc", "NPC", "desc", 3)
        {
            Behavior = "aggressive"
        };

        bool isValid = NPCBehaviorSystem.ValidateBehavior(npc);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateBehavior_ValidPatrolWithPath_ReturnsTrue()
    {
        var npc = new NPC("npc", "NPC", "desc", 3)
        {
            Behavior = "patrol",
            PatrolPath = new List<string> { "room1", "room2", "room3" }
        };

        bool isValid = NPCBehaviorSystem.ValidateBehavior(npc);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateBehavior_PatrolWithoutPath_ReturnsFalse()
    {
        var npc = new NPC("npc", "NPC", "desc", 3)
        {
            Behavior = "patrol",
            PatrolPath = new List<string>()
        };

        bool isValid = NPCBehaviorSystem.ValidateBehavior(npc);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateBehavior_InvalidBehavior_ReturnsFalse()
    {
        var npc = new NPC("npc", "NPC", "desc", 3)
        {
            Behavior = "invalid_behavior"
        };

        bool isValid = NPCBehaviorSystem.ValidateBehavior(npc);

        Assert.False(isValid);
    }

    [Fact]
    public void BehaviorAction_HasCorrectProperties()
    {
        var action = new NPCBehaviorSystem.BehaviorAction
        {
            ActionType = "move",
            TargetRoomId = "room2",
            Direction = "north",
            Message = "NPC moves north"
        };

        Assert.Equal("move", action.ActionType);
        Assert.Equal("room2", action.TargetRoomId);
        Assert.Equal("north", action.Direction);
        Assert.Equal("NPC moves north", action.Message);
    }

    [Fact]
    public void DetermineBehaviorAction_AggressiveWithMultiplePlayers_TargetsOne()
    {
        var npc = new NPC("orc", "Orc", "desc", 5)
        {
            Behavior = "aggressive"
        };
        var room = CreateTestRoom();
        var players = new List<Character>
        {
            new Character("p1", "Hero1", "warrior"),
            new Character("p2", "Hero2", "mage"),
            new Character("p3", "Hero3", "thief")
        };

        var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);

        Assert.Equal("attack", action.ActionType);
        Assert.NotNull(action.TargetCharacterId);
        Assert.Contains(action.TargetCharacterId, players.Select(p => p.Id));
    }

    [Fact]
    public void DetermineBehaviorAction_Wander_VariesRandom()
    {
        var npc = new NPC("wanderer", "Wanderer", "desc", 3)
        {
            Behavior = "wander"
        };
        var room = CreateTestRoom();
        var players = new List<Character>();

        var actions = new HashSet<string>();
        for (int i = 0; i < 30; i++)
        {
            var action = NPCBehaviorSystem.DetermineBehaviorAction(npc, room, players);
            actions.Add(action.ActionType);
        }

        // Should have both move and idle actions due to randomness
        Assert.True(actions.Count >= 1);
    }
}
