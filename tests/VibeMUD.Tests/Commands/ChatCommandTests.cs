namespace VibeMUD.Tests.Commands;

using Xunit;
using VibeMUD.Commands;
using VibeMUD.Commands.Built_in;
using VibeMUD.Core;
using VibeMUD.Models;

public class ChatCommandTests
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
    public void SayCommand_WithMessage_Succeeds()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };
        gameState.AddCharacter(character);

        var command = new SayCommand();
        var result = command.Execute(character, gameState, new[] { "Hello", "everyone!" });

        Assert.True(result.Success);
        Assert.Contains("says:", result.Message);
        Assert.Contains("Hello everyone!", result.Message);
    }

    [Fact]
    public void SayCommand_NoArgs_ReturnsError()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };

        var command = new SayCommand();
        var result = command.Execute(character, gameState, new string[] { });

        Assert.False(result.Success);
    }

    [Fact]
    public void SayCommand_SingleWord_Succeeds()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };
        gameState.AddCharacter(character);

        var command = new SayCommand();
        var result = command.Execute(character, gameState, new[] { "Hi" });

        Assert.True(result.Success);
        Assert.Contains("Hi", result.Message);
    }

    [Fact]
    public void SayCommand_HasCorrectName()
    {
        var command = new SayCommand();
        Assert.Equal("say", command.Name);
    }

    [Fact]
    public void SayCommand_HasAliases()
    {
        var command = new SayCommand();
        Assert.Contains("s", command.Aliases);
    }

    [Fact]
    public void ChatCommand_RoomScope_Succeeds()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };

        var command = new ChatCommand();
        var result = command.Execute(character, gameState, new[] { "room", "Hello", "from", "Alice" });

        Assert.True(result.Success);
        Assert.Contains("ROOM", result.Message);
    }

    [Fact]
    public void ChatCommand_AreaScope_Succeeds()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };

        var command = new ChatCommand();
        var result = command.Execute(character, gameState, new[] { "area", "Attention", "everyone!" });

        Assert.True(result.Success);
        Assert.Contains("AREA", result.Message);
    }

    [Fact]
    public void ChatCommand_GlobalScope_Succeeds()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };

        var command = new ChatCommand();
        var result = command.Execute(character, gameState, new[] { "global", "Big", "announcement" });

        Assert.True(result.Success);
        Assert.Contains("GLOBAL", result.Message);
    }

    [Fact]
    public void ChatCommand_InvalidScope_ReturnsError()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };

        var command = new ChatCommand();
        var result = command.Execute(character, gameState, new[] { "invalid", "message" });

        Assert.False(result.Success);
        Assert.Contains("Invalid scope", result.Message);
    }

    [Fact]
    public void ChatCommand_NoArgs_ReturnsError()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };

        var command = new ChatCommand();
        var result = command.Execute(character, gameState, new string[] { });

        Assert.False(result.Success);
    }

    [Fact]
    public void ChatCommand_OnlyScope_ReturnsError()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };

        var command = new ChatCommand();
        var result = command.Execute(character, gameState, new[] { "room" });

        Assert.False(result.Success);
    }

    [Fact]
    public void ChatCommand_HasCorrectName()
    {
        var command = new ChatCommand();
        Assert.Equal("chat", command.Name);
    }

    [Fact]
    public void ChatCommand_HasAliases()
    {
        var command = new ChatCommand();
        Assert.Contains("c", command.Aliases);
    }

    [Fact]
    public void SayCommand_LongMessage_Succeeds()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };
        gameState.AddCharacter(character);

        var longMessage = "This is a very long message with many words to test the say command";
        var args = longMessage.Split(' ');

        var command = new SayCommand();
        var result = command.Execute(character, gameState, args);

        Assert.True(result.Success);
        Assert.Contains(longMessage, result.Message);
    }

    [Fact]
    public void CommandHandler_CanExecuteSayCommand()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };
        gameState.AddCharacter(character);

        var handler = new CommandHandler();
        var result = handler.ExecuteCommand(character, gameState, "say Hello world");

        Assert.True(result.Success);
    }

    [Fact]
    public void CommandHandler_CanExecuteSayAlias()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };
        gameState.AddCharacter(character);

        var handler = new CommandHandler();
        var result = handler.ExecuteCommand(character, gameState, "s Test");

        Assert.True(result.Success);
    }

    [Fact]
    public void CommandHandler_CanExecuteChatCommand()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };
        gameState.AddCharacter(character);

        var handler = new CommandHandler();
        var result = handler.ExecuteCommand(character, gameState, "chat room Hello from room");

        Assert.True(result.Success);
    }

    [Fact]
    public void CommandHandler_CanExecuteChatAlias()
    {
        var gameState = CreateTestGameState();
        var character = new Character("player1", "Alice", "warrior")
        {
            CurrentAreaId = "test_area",
            CurrentRoomId = "test_room",
            Health = 100,
            MaxHealth = 100
        };
        gameState.AddCharacter(character);

        var handler = new CommandHandler();
        var result = handler.ExecuteCommand(character, gameState, "c global Announcement");

        Assert.True(result.Success);
    }
}
