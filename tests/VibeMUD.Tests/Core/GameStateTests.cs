namespace VibeMUD.Tests.Core;

using VibeMUD.Core;
using VibeMUD.Models;

public class GameStateTests
{
    private GameState CreateTestGameState()
    {
        var gameState = new GameState();

        // Add test area
        var area = new Area("test_area", "Test Area", "A test area", 1, 10);
        var room = new Room("test_room", "Test Room", "A test room");
        room.AddExit("north", "test_area", "room_north");
        area.AddRoom(room);
        gameState.Areas["test_area"] = area;

        // Add test character
        var character = new Character("test_char", "TestChar", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "test_room";
        gameState.AddCharacter(character);

        // Add test NPC
        var npc = new NPC("test_npc", "Test NPC", "A test NPC", 5);
        gameState.NPCs["test_npc"] = npc;

        return gameState;
    }

    [Fact]
    public void GetRoom_ValidAreaAndRoom_ReturnsRoom()
    {
        var gameState = CreateTestGameState();

        var room = gameState.GetRoom("test_area", "test_room");

        Assert.NotNull(room);
        Assert.Equal("Test Room", room!.Title);
    }

    [Fact]
    public void GetRoom_InvalidArea_ReturnsNull()
    {
        var gameState = CreateTestGameState();

        var room = gameState.GetRoom("invalid_area", "test_room");

        Assert.Null(room);
    }

    [Fact]
    public void GetRoom_InvalidRoom_ReturnsNull()
    {
        var gameState = CreateTestGameState();

        var room = gameState.GetRoom("test_area", "invalid_room");

        Assert.Null(room);
    }

    [Fact]
    public void GetCharacter_ValidId_ReturnsCharacter()
    {
        var gameState = CreateTestGameState();

        var character = gameState.GetCharacter("test_char");

        Assert.NotNull(character);
        Assert.Equal("TestChar", character!.Name);
    }

    [Fact]
    public void GetCharacter_InvalidId_ReturnsNull()
    {
        var gameState = CreateTestGameState();

        var character = gameState.GetCharacter("invalid_char");

        Assert.Null(character);
    }

    [Fact]
    public void GetNPC_ValidId_ReturnsNPC()
    {
        var gameState = CreateTestGameState();

        var npc = gameState.GetNPC("test_npc");

        Assert.NotNull(npc);
        Assert.Equal("Test NPC", npc!.Name);
    }

    [Fact]
    public void AddCharacter_NewCharacter_Added()
    {
        var gameState = new GameState();
        var character = new Character("player1", "Player One", "mage");

        gameState.AddCharacter(character);

        Assert.True(gameState.Characters.ContainsKey("player1"));
    }

    [Fact]
    public void RemoveCharacter_ExistingCharacter_Removed()
    {
        var gameState = CreateTestGameState();

        gameState.RemoveCharacter("test_char");

        Assert.False(gameState.Characters.ContainsKey("test_char"));
    }

    [Fact]
    public void RemoveCharacter_RemovesFromRoom()
    {
        var gameState = CreateTestGameState();
        var character = gameState.GetCharacter("test_char");
        var room = gameState.GetRoom(character!.CurrentAreaId!, character.CurrentRoomId!);
        Assert.NotNull(room);
        room!.AddPlayer("test_char");
        Assert.Contains("test_char", room.PlayerIds);

        gameState.RemoveCharacter("test_char");

        Assert.DoesNotContain("test_char", room.PlayerIds);
    }

    [Fact]
    public void SpawnNPC_ValidTemplate_CreatesNPC()
    {
        var gameState = CreateTestGameState();
        var npcTemplate = new NPC("goblin_template", "Goblin", "A goblin", 3);
        npcTemplate.Health = 20;
        npcTemplate.MaxHealth = 20;
        gameState.NPCs["goblin_template"] = npcTemplate;

        var spawned = gameState.SpawnNPC("goblin_template", "test_area", "test_room");

        Assert.NotNull(spawned);
        Assert.Equal("Goblin", spawned!.Name);
        Assert.Equal(3, spawned.Level);
        Assert.Equal(20, spawned.Health);
    }

    [Fact]
    public void SpawnNPC_InvalidTemplate_ReturnsNull()
    {
        var gameState = CreateTestGameState();

        var spawned = gameState.SpawnNPC("invalid_template", "test_area", "test_room");

        Assert.Null(spawned);
    }

    [Fact]
    public void GetNPCsInRoom_ReturnsNPCs()
    {
        var gameState = CreateTestGameState();
        var room = gameState.GetRoom("test_area", "test_room");
        room?.AddNPC("test_npc");

        var npcs = gameState.GetNPCsInRoom("test_area", "test_room");

        Assert.NotEmpty(npcs);
        Assert.Single(npcs);
        Assert.Equal("Test NPC", npcs[0].Name);
    }

    [Fact]
    public void GetPlayersInRoom_ReturnsPlayers()
    {
        var gameState = CreateTestGameState();
        var character = gameState.GetCharacter("test_char");
        var room = gameState.GetRoom("test_area", "test_room");
        room!.AddPlayer(character!.Id);

        var players = gameState.GetPlayersInRoom("test_area", "test_room");

        Assert.NotEmpty(players);
        Assert.Single(players);
        Assert.Equal("TestChar", players[0].Name);
    }

    [Fact]
    public void GetItemsInRoom_ReturnsItems()
    {
        var gameState = CreateTestGameState();
        var room = gameState.GetRoom("test_area", "test_room");
        var item = new Item("test_item", "Test Item", "equipment", "misc", 10);
        room?.AddItem(item);

        var items = gameState.GetItemsInRoom("test_area", "test_room");

        Assert.NotEmpty(items);
        Assert.Single(items);
        Assert.Equal("Test Item", items[0].Name);
    }
}
