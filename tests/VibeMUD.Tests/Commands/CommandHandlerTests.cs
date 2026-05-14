namespace VibeMUD.Tests.Commands;

using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Models;

public class CommandHandlerTests
{
    private GameState CreateTestGameState()
    {
        var gameState = new GameState();
        var area = new Area("test_area", "Test Area", "A test area", 1, 10);
        var room = new Room("test_room", "Test Room", "A test room");
        area.AddRoom(room);
        gameState.Areas["test_area"] = area;
        return gameState;
    }

    [Fact]
    public void ExecuteCommand_Help_Succeeds()
    {
        var handler = new CommandHandler();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "test_room";
        character.Health = 100;
        character.MaxHealth = 100;
        var gameState = CreateTestGameState();

        var result = handler.ExecuteCommand(character, gameState, "help");

        Assert.True(result.Success);
        Assert.Contains("VIBE MUD HELP", result.Message);
    }

    [Fact]
    public void ExecuteCommand_HelpAlias_Succeeds()
    {
        var handler = new CommandHandler();
        var character = new Character("player1", "Player", "warrior");
        character.Health = 100;
        character.MaxHealth = 100;
        var gameState = CreateTestGameState();

        var result = handler.ExecuteCommand(character, gameState, "h");

        Assert.True(result.Success);
        Assert.Contains("VIBE MUD HELP", result.Message);
    }

    [Fact]
    public void ExecuteCommand_UnknownCommand_Fails()
    {
        var handler = new CommandHandler();
        var character = new Character("player1", "Player", "warrior");
        var gameState = CreateTestGameState();

        var result = handler.ExecuteCommand(character, gameState, "nonexistent");

        Assert.False(result.Success);
        Assert.Contains("Unknown command", result.Message);
    }

    [Fact]
    public void ExecuteCommand_EmptyInput_Fails()
    {
        var handler = new CommandHandler();
        var character = new Character("player1", "Player", "warrior");
        var gameState = CreateTestGameState();

        var result = handler.ExecuteCommand(character, gameState, "");

        Assert.False(result.Success);
    }

    [Fact]
    public void ExecuteCommand_DeadCharacter_Fails()
    {
        var handler = new CommandHandler();
        var character = new Character("player1", "Player", "warrior");
        character.Health = 0;
        var gameState = CreateTestGameState();

        var result = handler.ExecuteCommand(character, gameState, "help");

        Assert.False(result.Success);
        Assert.Contains("dead", result.Message);
    }

    [Fact]
    public void RegisterCommand_CustomCommand_Registered()
    {
        var handler = new CommandHandler();
        var testCommand = new TestCommand();

        handler.RegisterCommand(testCommand);
        var retrieved = handler.GetCommand("test");

        Assert.NotNull(retrieved);
        Assert.Equal("test", retrieved!.Name);
    }

    [Fact]
    public void GetAllCommands_ReturnsAllCommands()
    {
        var handler = new CommandHandler();

        var commands = handler.GetAllCommands();

        Assert.NotEmpty(commands);
        Assert.Contains(commands, c => c.Name == "help");
        Assert.Contains(commands, c => c.Name == "look");
    }

    [Fact]
    public void ExecuteCommand_Look_ShowsRoom()
    {
        var handler = new CommandHandler();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "test_room";
        character.Health = 100;
        character.MaxHealth = 100;
        var gameState = CreateTestGameState();

        var result = handler.ExecuteCommand(character, gameState, "look");

        Assert.True(result.Success);
        Assert.Contains("Test Room", result.Message);
    }

    [Fact]
    public void ExecuteCommand_Info_ShowsCharacterInfo()
    {
        var handler = new CommandHandler();
        var character = new Character("player1", "TestPlayer", "warrior");
        character.Health = 100;
        character.MaxHealth = 100;
        var gameState = CreateTestGameState();

        var result = handler.ExecuteCommand(character, gameState, "info");

        Assert.True(result.Success);
        Assert.Contains("TestPlayer", result.Message);
        Assert.Contains("warrior", result.Message);
    }

    // Test command for testing
    private class TestCommand : Command
    {
        public TestCommand()
        {
            Name = "test";
            Description = "Test command";
        }

        public override CommandResult Execute(Character character, GameState gameState, string[] args)
        {
            return CommandResult.Ok("Test executed");
        }
    }
}
