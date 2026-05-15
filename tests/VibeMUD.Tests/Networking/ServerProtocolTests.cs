namespace VibeMUD.Tests.Networking;

using Xunit;
using VibeMUD.Networking;

public class ServerProtocolTests
{
    [Fact]
    public void SerializeMessage_CreatesValidJson()
    {
        var message = new ServerMessage
        {
            Type = MessageType.Response,
            PlayerId = "test-player",
            Success = true,
            Message = "Test message"
        };

        var json = ServerProtocol.SerializeMessage(message);

        Assert.NotNull(json);
        Assert.Contains("\"type\":\"response\"", json);
        Assert.Contains("\"playerId\":\"test-player\"", json);
        Assert.Contains("\"success\":true", json);
        Assert.Contains("\"message\":\"Test message\"", json);
    }

    [Fact]
    public void DeserializeClientMessage_ParsesValidJson()
    {
        var json = "{\"messageId\":\"msg-1\",\"type\":\"command\",\"playerId\":\"player-1\",\"command\":\"look\",\"args\":[]}";

        var message = ServerProtocol.DeserializeClientMessage(json);

        Assert.NotNull(message);
        Assert.Equal("msg-1", message.MessageId);
        Assert.Equal(MessageType.Command, message.Type);
        Assert.Equal("player-1", message.PlayerId);
        Assert.Equal("look", message.Command);
    }

    [Fact]
    public void DeserializeClientMessage_InvalidJson_ReturnsNull()
    {
        var json = "not valid json";

        var message = ServerProtocol.DeserializeClientMessage(json);

        Assert.Null(message);
    }

    [Fact]
    public void CreateResponse_Success_CreatesValidMessage()
    {
        var response = ServerProtocol.CreateResponse("player-1", true, "Command executed");

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        Assert.Equal("player-1", response.PlayerId);
        Assert.True(response.Success);
        Assert.Equal("Command executed", response.Message);
    }

    [Fact]
    public void CreateResponse_Error_CreatesValidMessage()
    {
        var response = ServerProtocol.CreateResponse("player-1", false, "Command failed");

        Assert.NotNull(response);
        Assert.Equal(MessageType.Response, response.Type);
        Assert.False(response.Success);
        Assert.Equal("Command failed", response.Message);
    }

    [Fact]
    public void CreateNotification_CreatesValidMessage()
    {
        var notification = ServerProtocol.CreateNotification("player-1", "You have leveled up!");

        Assert.NotNull(notification);
        Assert.Equal(MessageType.Notification, notification.Type);
        Assert.Equal("player-1", notification.PlayerId);
        Assert.True(notification.Success);
        Assert.Equal("You have leveled up!", notification.Message);
    }

    [Fact]
    public void CreateError_CreatesValidMessage()
    {
        var error = ServerProtocol.CreateError("player-1", "Unknown command");

        Assert.NotNull(error);
        Assert.Equal(MessageType.Error, error.Type);
        Assert.Equal("player-1", error.PlayerId);
        Assert.False(error.Success);
        Assert.Equal("Unknown command", error.Message);
    }

    [Fact]
    public void CreateChatMessage_CreatesValidMessage()
    {
        var chat = ServerProtocol.CreateChatMessage("player-1", "Aragorn", "Hello everyone!", "room");

        Assert.NotNull(chat);
        Assert.Equal(MessageType.Chat, chat.Type);
        Assert.True(chat.Success);
        Assert.NotNull(chat.Data);
    }

    [Fact]
    public void RoundTripSerialization_PreservesData()
    {
        var original = new ClientMessage
        {
            MessageId = "msg-123",
            Type = MessageType.Command,
            PlayerId = "player-1",
            Command = "attack",
            Args = new[] { "goblin_1" }
        };

        var json = ServerProtocol.SerializeMessage(original);
        var deserialized = ServerProtocol.DeserializeClientMessage(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.MessageId, deserialized.MessageId);
        Assert.Equal(original.Type, deserialized.Type);
        Assert.Equal(original.PlayerId, deserialized.PlayerId);
        Assert.Equal(original.Command, deserialized.Command);
    }

    [Fact]
    public void MessageType_HasAllConstants()
    {
        Assert.NotNull(MessageType.Command);
        Assert.NotNull(MessageType.Response);
        Assert.NotNull(MessageType.Chat);
        Assert.NotNull(MessageType.RoomState);
        Assert.NotNull(MessageType.Error);
        Assert.NotNull(MessageType.Connect);
    }

    [Fact]
    public void ServerMessage_HasValidTimestamp()
    {
        var message = ServerProtocol.CreateResponse("player-1", true, "Test");
        var beforeTime = DateTime.UtcNow.AddSeconds(-1);
        var afterTime = DateTime.UtcNow.AddSeconds(1);

        Assert.True(message.Timestamp > beforeTime && message.Timestamp < afterTime);
    }

    [Fact]
    public void ClientMessage_DefaultMessageIdIsUnique()
    {
        var msg1 = new ClientMessage();
        var msg2 = new ClientMessage();

        Assert.NotEqual(msg1.MessageId, msg2.MessageId);
    }

    [Fact]
    public void RoomStateUpdate_ContainsAllProperties()
    {
        var update = new RoomStateUpdate
        {
            AreaId = "test_area",
            RoomId = "test_room",
            RoomTitle = "A Test Room",
            RoomDescription = "A room for testing"
        };

        Assert.Equal(MessageType.RoomState, update.Type);
        Assert.Equal("test_area", update.AreaId);
        Assert.Equal("test_room", update.RoomId);
        Assert.Equal("A Test Room", update.RoomTitle);
        Assert.NotNull(update.PlayersInRoom);
        Assert.NotNull(update.NPCsInRoom);
        Assert.NotNull(update.Exits);
    }

    [Fact]
    public void PlayerInfo_CanBeCreated()
    {
        var player = new PlayerInfo
        {
            Id = "player-1",
            Name = "Aragorn",
            Class = "warrior",
            Level = 5,
            Health = 50,
            MaxHealth = 100
        };

        Assert.Equal("player-1", player.Id);
        Assert.Equal("Aragorn", player.Name);
        Assert.Equal("warrior", player.Class);
        Assert.Equal(5, player.Level);
    }

    [Fact]
    public void NPCInfo_CanBeCreated()
    {
        var npc = new NPCInfo
        {
            Id = "goblin_1",
            Name = "Goblin",
            Level = 3,
            Health = 15,
            MaxHealth = 15
        };

        Assert.Equal("goblin_1", npc.Id);
        Assert.Equal("Goblin", npc.Name);
        Assert.Equal(3, npc.Level);
    }
}
