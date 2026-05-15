namespace VibeMUD.Tests.Networking;

using Xunit;
using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Models;
using VibeMUD.Networking;

public class ServerGameStateTests
{
    private ServerGameState CreateTestServerGameState()
    {
        var gameState = new GameState();
        var area = new Area("test_area", "Test Area", "A test area", 1, 10);
        var room = new Room("test_room", "Test Room", "A test room");
        area.AddRoom(room);
        gameState.Areas["test_area"] = area;

        var character = new Character("test_player", "TestPlayer", "warrior")
        {
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room"
        };
        gameState.AddCharacter(character);
        room.AddPlayer("test_player");

        var commandHandler = new CommandHandler();
        return new ServerGameState(gameState, commandHandler);
    }

    [Fact]
    public void PlayerConnectionRegistry_WorksCorrectly()
    {
        var serverGameState = CreateTestServerGameState();
        var players = serverGameState.GetConnectedPlayers();

        // Initially no players registered beyond initial state
        Assert.NotNull(players);
    }


    [Fact]
    public async Task ExecuteCommandAsync_ExecutesCommand()
    {
        var serverGameState = CreateTestServerGameState();

        var result = await serverGameState.ExecuteCommandAsync("test_player", "help");

        Assert.True(result.Success);
    }

    [Fact]
    public async Task AddCharacterAsync_AddsCharacter()
    {
        var serverGameState = CreateTestServerGameState();
        var newChar = new Character("new_player", "NewPlayer", "thief")
        {
            Level = 1,
            Health = 80,
            MaxHealth = 80,
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room"
        };

        await serverGameState.AddCharacterAsync(newChar);

        var retrieved = serverGameState.GetCharacter("new_player");
        Assert.NotNull(retrieved);
        Assert.Equal("NewPlayer", retrieved.Name);
    }

    [Fact]
    public async Task RemoveCharacterAsync_RemovesCharacter()
    {
        var serverGameState = CreateTestServerGameState();

        await serverGameState.RemoveCharacterAsync("test_player");

        var retrieved = serverGameState.GetCharacter("test_player");
        Assert.Null(retrieved);
    }

    [Fact]
    public void GetCharacter_ReturnsCharacter()
    {
        var serverGameState = CreateTestServerGameState();

        var character = serverGameState.GetCharacter("test_player");

        Assert.NotNull(character);
        Assert.Equal("TestPlayer", character.Name);
    }

    [Fact]
    public void GetRoom_ReturnsRoom()
    {
        var serverGameState = CreateTestServerGameState();

        var room = serverGameState.GetRoom("test_area", "test_room");

        Assert.NotNull(room);
        Assert.Equal("Test Room", room.Title);
    }

    [Fact]
    public void GetPlayersInRoom_ReturnsPlayerList()
    {
        var serverGameState = CreateTestServerGameState();

        var players = serverGameState.GetPlayersInRoom("test_area", "test_room");

        Assert.NotNull(players);
        Assert.Single(players);
        Assert.Equal("TestPlayer", players[0].Name);
    }

    [Fact]
    public void GetNPCsInRoom_ReturnsEmptyListWhenNoNPCs()
    {
        var serverGameState = CreateTestServerGameState();

        var npcs = serverGameState.GetNPCsInRoom("test_area", "test_room");

        Assert.NotNull(npcs);
        Assert.Empty(npcs);
    }

    [Fact]
    public void GetItemsInRoom_ReturnsEmptyListWhenNoItems()
    {
        var serverGameState = CreateTestServerGameState();

        var items = serverGameState.GetItemsInRoom("test_area", "test_room");

        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public void GetGameState_ReturnsUnderlyingGameState()
    {
        var serverGameState = CreateTestServerGameState();

        var gameState = serverGameState.GetGameState();

        Assert.NotNull(gameState);
        Assert.NotEmpty(gameState.Characters);
    }

    [Fact]
    public void ThreadSafety_MultipleReadAccess()
    {
        var serverGameState = CreateTestServerGameState();
        var tasks = new Task[10];

        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var character = serverGameState.GetCharacter("test_player");
                Assert.NotNull(character);
            });
        }

        Task.WaitAll(tasks);
    }
}

