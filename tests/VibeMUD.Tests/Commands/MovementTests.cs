namespace VibeMUD.Tests.Commands;

using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Models;

public class MovementTests
{
    private GameState CreateTestWorld()
    {
        var gameState = new GameState();

        // Create area with connected rooms
        var area = new Area("test_area", "Test Area", "A test area", 1, 10);

        var room1 = new Room("room_1", "Room One", "The first room");
        var room2 = new Room("room_2", "Room Two", "The second room");
        var room3 = new Room("room_3", "Room Three", "The third room");

        // Connect rooms
        room1.AddExit("north", "test_area", "room_2");
        room1.AddExit("east", "test_area", "room_3");

        room2.AddExit("south", "test_area", "room_1");

        room3.AddExit("west", "test_area", "room_1");

        area.AddRoom(room1);
        area.AddRoom(room2);
        area.AddRoom(room3);

        gameState.Areas["test_area"] = area;

        return gameState;
    }

    [Fact]
    public void MoveNorth_ValidExit_Succeeds()
    {
        var gameState = CreateTestWorld();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "room_1";
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "room_1")?.AddPlayer(character.Id);

        var result = MoveCommand.Move(character, gameState, "north");

        Assert.True(result.Success);
        Assert.Equal("test_area", character.CurrentAreaId);
        Assert.Equal("room_2", character.CurrentRoomId);
    }

    [Fact]
    public void MoveSouth_ValidExit_Succeeds()
    {
        var gameState = CreateTestWorld();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "room_2";
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "room_2")?.AddPlayer(character.Id);

        var result = MoveCommand.Move(character, gameState, "south");

        Assert.True(result.Success);
        Assert.Equal("room_1", character.CurrentRoomId);
    }

    [Fact]
    public void MoveEast_ValidExit_Succeeds()
    {
        var gameState = CreateTestWorld();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "room_1";
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "room_1")?.AddPlayer(character.Id);

        var result = MoveCommand.Move(character, gameState, "east");

        Assert.True(result.Success);
        Assert.Equal("room_3", character.CurrentRoomId);
    }

    [Fact]
    public void MoveWest_ValidExit_Succeeds()
    {
        var gameState = CreateTestWorld();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "room_3";
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "room_3")?.AddPlayer(character.Id);

        var result = MoveCommand.Move(character, gameState, "west");

        Assert.True(result.Success);
        Assert.Equal("room_1", character.CurrentRoomId);
    }

    [Fact]
    public void Move_BlockedExit_Fails()
    {
        var gameState = CreateTestWorld();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "room_2";
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "room_2")?.AddPlayer(character.Id);

        var result = MoveCommand.Move(character, gameState, "east");

        Assert.False(result.Success);
        Assert.Contains("cannot go", result.Message);
        Assert.Equal("room_2", character.CurrentRoomId);
    }

    [Fact]
    public void Move_InvalidDirection_Fails()
    {
        var gameState = CreateTestWorld();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "room_1";

        var result = MoveCommand.Move(character, gameState, "invalid");

        Assert.False(result.Success);
        Assert.Contains("Invalid direction", result.Message);
    }

    [Fact]
    public void Move_RemovesFromOldRoom()
    {
        var gameState = CreateTestWorld();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "room_1";
        gameState.AddCharacter(character);
        var oldRoom = gameState.GetRoom("test_area", "room_1");
        oldRoom?.AddPlayer(character.Id);

        MoveCommand.Move(character, gameState, "north");

        Assert.DoesNotContain(character.Id, oldRoom!.PlayerIds);
    }

    [Fact]
    public void Move_AddsToNewRoom()
    {
        var gameState = CreateTestWorld();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "room_1";
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "room_1")?.AddPlayer(character.Id);

        MoveCommand.Move(character, gameState, "north");

        var newRoom = gameState.GetRoom("test_area", "room_2");
        Assert.Contains(character.Id, newRoom!.PlayerIds);
    }

    [Fact]
    public void NorthCommand_Executes()
    {
        var gameState = CreateTestWorld();
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "room_1";
        gameState.AddCharacter(character);
        gameState.GetRoom("test_area", "room_1")?.AddPlayer(character.Id);
        var cmd = new NorthCommand();

        var result = cmd.Execute(character, gameState, new string[] { });

        Assert.True(result.Success);
        Assert.Equal("room_2", character.CurrentRoomId);
    }
}
