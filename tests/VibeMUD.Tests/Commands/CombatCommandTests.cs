namespace VibeMUD.Tests.Commands;

using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Models;

public class CombatCommandTests
{
    private GameState CreateCombatTestWorld()
    {
        var gameState = new GameState();

        var area = new Area("test_area", "Test Area", "A test area", 1, 10);
        var room = new Room("test_room", "Test Room", "A test room");
        area.AddRoom(room);
        gameState.Areas["test_area"] = area;

        // Add test NPC
        var goblin = new NPC("goblin_1", "Goblin", "A green goblin", 3)
        {
            Health = 20,
            MaxHealth = 20,
            Mana = 5,
            MaxMana = 5,
            Strength = 10,
            Dexterity = 10,
            Constitution = 10,
            Intelligence = 8,
            Wisdom = 9,
            Charisma = 7
        };
        gameState.NPCs["goblin_1"] = goblin;
        room.AddNPC("goblin_1");

        // Add test skill
        var fireball = new Skill("fireball", "Fireball", "Fire spell", 20, 5, 15, "fire", true);
        gameState.Skills["fireball"] = fireball;

        return gameState;
    }

    [Fact]
    public void AttackCommand_ValidTarget_AttacksNPC()
    {
        var gameState = CreateCombatTestWorld();
        var character = new Character("player1", "Player", "warrior")
        {
            Health = 100,
            MaxHealth = 100,
            Strength = 16,
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room"
        };
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "test_room")?.AddPlayer(character.Id);
        var cmd = new AttackCommand();

        var result = cmd.Execute(character, gameState, new[] { "Goblin" });

        Assert.True(result.Success);
        Assert.Contains("attack", result.Message.ToLower());
    }

    [Fact]
    public void AttackCommand_NoTarget_Fails()
    {
        var gameState = CreateCombatTestWorld();
        var character = new Character("player1", "Player", "warrior")
        {
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room"
        };
        var cmd = new AttackCommand();

        var result = cmd.Execute(character, gameState, new string[] { });

        Assert.False(result.Success);
        Assert.Contains("Usage", result.Message);
    }

    [Fact]
    public void AttackCommand_InvalidTarget_Fails()
    {
        var gameState = CreateCombatTestWorld();
        var character = new Character("player1", "Player", "warrior")
        {
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room"
        };
        gameState.AddCharacter(character);
        var cmd = new AttackCommand();

        var result = cmd.Execute(character, gameState, new[] { "Dragon" });

        Assert.False(result.Success);
        Assert.Contains("don't see", result.Message);
    }

    [Fact]
    public void AttackCommand_DealsDamageToNPC()
    {
        var gameState = CreateCombatTestWorld();
        var character = new Character("player1", "Player", "warrior")
        {
            Health = 100,
            MaxHealth = 100,
            Strength = 16,
            Gold = 0,
            ExperiencePoints = 0,
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room"
        };
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "test_room")?.AddPlayer(character.Id);

        var goblin = gameState.NPCs["goblin_1"];
        int initialHealth = goblin.Health;

        var cmd = new AttackCommand();

        var result = cmd.Execute(character, gameState, new[] { "Goblin" });

        // Should deal damage
        Assert.True(result.Success);
        Assert.True(goblin.Health < initialHealth);
    }

    [Fact]
    public void CastCommand_ValidSkill_CastsSuccessfully()
    {
        var gameState = CreateCombatTestWorld();
        var character = new Character("player1", "Player", "mage")
        {
            Health = 100,
            MaxHealth = 100,
            Mana = 50,
            MaxMana = 50,
            Intelligence = 16,
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room"
        };
        character.LearnSkill("fireball");
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "test_room")?.AddPlayer(character.Id);
        var cmd = new CastCommand();

        var result = cmd.Execute(character, gameState, new[] { "Fireball" });

        Assert.True(result.Success);
        Assert.Contains("cast", result.Message.ToLower());
        Assert.True(character.Mana < 50); // Mana consumed
    }

    [Fact]
    public void CastCommand_UnknownSkill_Fails()
    {
        var gameState = CreateCombatTestWorld();
        var character = new Character("player1", "Player", "mage")
        {
            Health = 100,
            MaxHealth = 100,
            Mana = 50,
            MaxMana = 50,
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room"
        };
        gameState.AddCharacter(character);
        var cmd = new CastCommand();

        var result = cmd.Execute(character, gameState, new[] { "UnknownSkill" });

        Assert.False(result.Success);
        Assert.Contains("don't know", result.Message);
    }

    [Fact]
    public void CastCommand_SkillNotLearned_Fails()
    {
        var gameState = CreateCombatTestWorld();
        var character = new Character("player1", "Player", "mage")
        {
            Health = 100,
            MaxHealth = 100,
            Mana = 50,
            MaxMana = 50,
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room"
        };
        // Don't learn the skill
        gameState.AddCharacter(character);
        var cmd = new CastCommand();

        var result = cmd.Execute(character, gameState, new[] { "Fireball" });

        Assert.False(result.Success);
        Assert.Contains("haven't learned", result.Message);
    }

    [Fact]
    public void UseCommand_Potion_HealCharacter()
    {
        var gameState = CreateCombatTestWorld();
        var potion = new Item("health_potion", "Health Potion", "potion", "consumable", 10);
        potion.Stats["heal"] = 30;

        var character = new Character("player1", "Player", "warrior")
        {
            Health = 70,
            MaxHealth = 100
        };
        character.AddToInventory(potion);
        var cmd = new UseCommand();

        var result = cmd.Execute(character, gameState, new[] { "Health Potion" });

        Assert.True(result.Success);
        Assert.Equal(100, character.Health);
        Assert.DoesNotContain(potion, character.Inventory);
    }

    [Fact]
    public void UseCommand_NoItem_Fails()
    {
        var gameState = CreateCombatTestWorld();
        var character = new Character("player1", "Player", "warrior");
        var cmd = new UseCommand();

        var result = cmd.Execute(character, gameState, new[] { "potion" });

        Assert.False(result.Success);
        Assert.Contains("don't have", result.Message);
    }

    [Fact]
    public void FleeCommand_AttemptsFlee()
    {
        var gameState = CreateCombatTestWorld();
        var character = new Character("player1", "Player", "thief")
        {
            Health = 100,
            MaxHealth = 100,
            Dexterity = 16,
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room"
        };
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "test_room")?.AddPlayer(character.Id);
        var cmd = new FleeCommand();

        var result = cmd.Execute(character, gameState, new string[] { });

        Assert.True(result.Success);
        Assert.True(result.Message.Contains("flee") || result.Message.Contains("escape") || result.Message.Contains("Failed"));
    }

    [Fact]
    public void FleeCommand_NoEnemies_Fails()
    {
        var gameState = new GameState();
        var area = new Area("test", "Test", "Test", 1, 10);
        var room = new Room("test", "Test", "Test");
        area.AddRoom(room);
        gameState.Areas["test"] = area;

        var character = new Character("player1", "Player", "warrior")
        {
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test",
            CurrentRoomId = "test"
        };
        gameState.AddCharacter(character);
        var cmd = new FleeCommand();

        var result = cmd.Execute(character, gameState, new string[] { });

        Assert.False(result.Success);
        Assert.Contains("nothing to flee", result.Message);
    }
}
