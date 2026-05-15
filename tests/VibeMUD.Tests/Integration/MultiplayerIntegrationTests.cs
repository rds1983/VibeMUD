namespace VibeMUD.Tests.Integration;

using Xunit;
using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Models;
using VibeMUD.Networking;

public class MultiplayerIntegrationTests
{
    private GameState CreateTestGameState()
    {
        var gameState = new GameState();

        // Create test area
        var area = new Area("test_area", "Test Area", "A test area for multiplayer", 1, 10);
        var room1 = new Room("room_1", "First Room", "A room with two exits");
        var room2 = new Room("room_2", "Second Room", "Another room");

        room1.AddExit("east", "test_area", "room_2");
        room2.AddExit("west", "test_area", "room_1");

        area.AddRoom(room1);
        area.AddRoom(room2);
        gameState.Areas["test_area"] = area;

        return gameState;
    }

    [Fact]
    public void TwoPlayersInSameRoom_BothAreVisible()
    {
        var gameState = CreateTestGameState();

        var player1 = new Character("player1", "Alice", "warrior")
        {
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_1"
        };

        var player2 = new Character("player2", "Bob", "thief")
        {
            Level = 1,
            Health = 80,
            MaxHealth = 80,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_1"
        };

        gameState.AddCharacter(player1);
        gameState.AddCharacter(player2);

        var room = gameState.GetRoom("test_area", "room_1");
        room?.AddPlayer("player1");
        room?.AddPlayer("player2");

        var players = gameState.GetPlayersInRoom("test_area", "room_1");

        Assert.Equal(2, players.Count);
        Assert.Contains(players, p => p.Name == "Alice");
        Assert.Contains(players, p => p.Name == "Bob");
    }

    [Fact]
    public void PlayerMovement_UpdatesRoomOccupants()
    {
        var gameState = CreateTestGameState();

        var player = new Character("player1", "Alice", "warrior")
        {
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_1"
        };

        gameState.AddCharacter(player);
        var room1 = gameState.GetRoom("test_area", "room_1");
        room1?.AddPlayer("player1");

        Assert.Single(room1?.PlayerIds ?? new List<string>());

        // Move player
        room1?.RemovePlayer("player1");
        player.CurrentRoomId = "room_2";
        var room2 = gameState.GetRoom("test_area", "room_2");
        room2?.AddPlayer("player1");

        Assert.Empty(room1?.PlayerIds ?? new List<string>());
        Assert.Single(room2?.PlayerIds ?? new List<string>());
    }

    [Fact]
    public void MultiplePlayersInDifferentRooms_NotVisible()
    {
        var gameState = CreateTestGameState();

        var player1 = new Character("player1", "Alice", "warrior")
        {
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_1"
        };

        var player2 = new Character("player2", "Bob", "thief")
        {
            Level = 1,
            Health = 80,
            MaxHealth = 80,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_2"
        };

        gameState.AddCharacter(player1);
        gameState.AddCharacter(player2);

        var room1 = gameState.GetRoom("test_area", "room_1");
        var room2 = gameState.GetRoom("test_area", "room_2");
        room1?.AddPlayer("player1");
        room2?.AddPlayer("player2");

        var playersInRoom1 = gameState.GetPlayersInRoom("test_area", "room_1");
        var playersInRoom2 = gameState.GetPlayersInRoom("test_area", "room_2");

        Assert.Single(playersInRoom1);
        Assert.Single(playersInRoom2);
        Assert.NotEqual(playersInRoom1[0].Id, playersInRoom2[0].Id);
    }

    [Fact]
    public void PlayerDisconnection_RemovesFromGame()
    {
        var gameState = CreateTestGameState();

        var player = new Character("player1", "Alice", "warrior")
        {
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_1"
        };

        gameState.AddCharacter(player);
        var room = gameState.GetRoom("test_area", "room_1");
        room?.AddPlayer("player1");

        Assert.NotNull(gameState.GetCharacter("player1"));

        // Disconnect
        gameState.RemoveCharacter("player1");

        Assert.Null(gameState.GetCharacter("player1"));
    }

    [Fact]
    public async Task RoomStateUpdate_IncludesAllRoomElements()
    {
        var gameState = CreateTestGameState();
        var commandHandler = new CommandHandler();
        var serverGameState = new ServerGameState(gameState, commandHandler);

        var player = new Character("player1", "Alice", "warrior")
        {
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_1"
        };

        gameState.AddCharacter(player);
        var room = gameState.GetRoom("test_area", "room_1");
        room?.AddPlayer("player1");

        var update = serverGameState.CreateRoomStateUpdate(player);

        Assert.Equal("test_area", update.AreaId);
        Assert.Equal("room_1", update.RoomId);
        Assert.NotNull(update.PlayersInRoom);
        Assert.NotNull(update.NPCsInRoom);
        Assert.NotNull(update.Exits);
    }

    [Fact]
    public void RoomExits_PresentInRoomState()
    {
        var gameState = CreateTestGameState();
        var commandHandler = new CommandHandler();
        var serverGameState = new ServerGameState(gameState, commandHandler);

        var player = new Character("player1", "Alice", "warrior")
        {
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_1"
        };

        gameState.AddCharacter(player);

        var update = serverGameState.CreateRoomStateUpdate(player);

        Assert.NotEmpty(update.Exits);
        Assert.True(update.Exits.ContainsKey("east"));
    }

    [Fact]
    public void ServerGameState_ThreadSafe_ConcurrentReads()
    {
        var gameState = CreateTestGameState();
        var commandHandler = new CommandHandler();
        var serverGameState = new ServerGameState(gameState, commandHandler);

        var player = new Character("player1", "Alice", "warrior")
        {
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_1"
        };

        gameState.AddCharacter(player);

        var tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                var retrieved = serverGameState.GetCharacter("player1");
                Assert.NotNull(retrieved);
            });
        }

        Task.WaitAll(tasks);
    }

    [Fact]
    public async Task Protocol_RoundTrip_CommandAndResponse()
    {
        var clientMsg = new ClientMessage
        {
            MessageId = "msg-1",
            Type = MessageType.Command,
            PlayerId = "player1",
            Command = "look",
            Args = new string[] { }
        };

        var json = ServerProtocol.SerializeMessage(clientMsg);
        var deserialized = ServerProtocol.DeserializeClientMessage(json);

        Assert.NotNull(deserialized);
        Assert.Equal("player1", deserialized.PlayerId);
        Assert.Equal("look", deserialized.Command);

        // Create response
        var response = ServerProtocol.CreateResponse("player1", true, "You are in a room");
        var responseJson = ServerProtocol.SerializeMessage(response);

        Assert.Contains("player1", responseJson);
        Assert.Contains("true", responseJson.ToLower());
    }

    [Fact]
    public void ChatMessage_Protocol_CreatesValidMessage()
    {
        var chatMsg = ServerProtocol.CreateChatMessage("player1", "Alice", "Hello everyone!", "room");

        Assert.Equal(MessageType.Chat, chatMsg.Type);
        Assert.True(chatMsg.Success);
        Assert.NotNull(chatMsg.Data);

        var json = ServerProtocol.SerializeMessage(chatMsg);
        Assert.Contains("room", json);
        Assert.Contains("Hello everyone!", json);
    }

    [Fact]
    public void PlayerInfo_Synchronization_AllPropertiesIncluded()
    {
        var player = new PlayerInfo
        {
            Id = "player1",
            Name = "Alice",
            Class = "warrior",
            Level = 5,
            Health = 50,
            MaxHealth = 100,
            InCombat = false
        };

        var json = ServerProtocol.SerializeMessage(player);

        Assert.Contains("\"id\":\"player1\"", json);
        Assert.Contains("\"name\":\"Alice\"", json);
        Assert.Contains("\"class\":\"warrior\"", json);
        Assert.Contains("\"level\":5", json);
        Assert.Contains("\"health\":50", json);
    }

    [Fact]
    public void NPC_Visibility_InRoomState()
    {
        var gameState = CreateTestGameState();
        var commandHandler = new CommandHandler();
        var serverGameState = new ServerGameState(gameState, commandHandler);

        var npc = new NPC("goblin_1", "Goblin", "A nasty goblin", 2)
        {
            Health = 10,
            MaxHealth = 10
        };

        gameState.NPCs["goblin_1"] = npc;
        var room = gameState.GetRoom("test_area", "room_1");
        room?.AddNPC("goblin_1");

        var player = new Character("player1", "Alice", "warrior")
        {
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_1"
        };

        gameState.AddCharacter(player);

        var update = serverGameState.CreateRoomStateUpdate(player);

        Assert.NotEmpty(update.NPCsInRoom);
        Assert.Contains(update.NPCsInRoom, n => n.Id == "goblin_1");
    }

    [Fact]
    public void Item_Visibility_InRoomState()
    {
        var gameState = CreateTestGameState();
        var commandHandler = new CommandHandler();
        var serverGameState = new ServerGameState(gameState, commandHandler);

        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50)
        {
            Weight = 3.5
        };

        var room = gameState.GetRoom("test_area", "room_1");
        room?.Items.Add(item);

        var player = new Character("player1", "Alice", "warrior")
        {
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            CurrentAreaId = "test_area",
            CurrentRoomId = "room_1"
        };

        gameState.AddCharacter(player);

        var update = serverGameState.CreateRoomStateUpdate(player);

        Assert.NotEmpty(update.ItemsInRoom);
        Assert.Contains(update.ItemsInRoom, i => i.Id == "iron_sword");
    }
}
