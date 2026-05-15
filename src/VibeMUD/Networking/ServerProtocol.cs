namespace VibeMUD.Networking;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Protocol message types for client-server communication
/// </summary>
public static class MessageType
{
    public const string Command = "command";
    public const string Response = "response";
    public const string Update = "update";
    public const string RoomState = "roomState";
    public const string Chat = "chat";
    public const string PlayerList = "playerList";
    public const string Notification = "notification";
    public const string Error = "error";
    public const string Connect = "connect";
    public const string Disconnect = "disconnect";
}

/// <summary>
/// Protocol message sent from client to server
/// </summary>
public class ClientMessage
{
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("playerName")]
    public string? PlayerName { get; set; }

    [JsonPropertyName("command")]
    public string? Command { get; set; }

    [JsonPropertyName("args")]
    public string[]? Args { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; } // "room", "area", "global"

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("characterClass")]
    public string? CharacterClass { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }
}

/// <summary>
/// Protocol message sent from server to client
/// </summary>
public class ServerMessage
{
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("playerId")]
    public string? PlayerId { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("data")]
    public JsonElement? Data { get; set; }
}

/// <summary>
/// Room state update sent to all players in a room
/// </summary>
public class RoomStateUpdate
{
    [JsonPropertyName("type")]
    public string Type { get; } = MessageType.RoomState;

    [JsonPropertyName("areaId")]
    public string AreaId { get; set; } = string.Empty;

    [JsonPropertyName("roomId")]
    public string RoomId { get; set; } = string.Empty;

    [JsonPropertyName("roomTitle")]
    public string RoomTitle { get; set; } = string.Empty;

    [JsonPropertyName("roomDescription")]
    public string RoomDescription { get; set; } = string.Empty;

    [JsonPropertyName("playersInRoom")]
    public List<PlayerInfo> PlayersInRoom { get; set; } = new();

    [JsonPropertyName("npcsInRoom")]
    public List<NPCInfo> NPCsInRoom { get; set; } = new();

    [JsonPropertyName("itemsInRoom")]
    public List<ItemInfo> ItemsInRoom { get; set; } = new();

    [JsonPropertyName("exits")]
    public Dictionary<string, string> Exits { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Minimal player information for sync
/// </summary>
public class PlayerInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("class")]
    public string Class { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("health")]
    public int Health { get; set; }

    [JsonPropertyName("maxHealth")]
    public int MaxHealth { get; set; }

    [JsonPropertyName("inCombat")]
    public bool InCombat { get; set; }
}

/// <summary>
/// Minimal NPC information for sync
/// </summary>
public class NPCInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("health")]
    public int Health { get; set; }

    [JsonPropertyName("maxHealth")]
    public int MaxHealth { get; set; }

    [JsonPropertyName("inCombat")]
    public bool InCombat { get; set; }
}

/// <summary>
/// Minimal item information for sync
/// </summary>
public class ItemInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Protocol serialization helper
/// </summary>
public static class ServerProtocol
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serialize object to JSON string
    /// </summary>
    public static string SerializeMessage(object message)
    {
        return JsonSerializer.Serialize(message, JsonOptions);
    }

    /// <summary>
    /// Deserialize JSON string to ClientMessage
    /// </summary>
    public static ClientMessage? DeserializeClientMessage(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ClientMessage>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Deserialize JSON string to ServerMessage
    /// </summary>
    public static ServerMessage? DeserializeServerMessage(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ServerMessage>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Create a command response message
    /// </summary>
    public static ServerMessage CreateResponse(string playerId, bool success, string message)
    {
        return new ServerMessage
        {
            Type = MessageType.Response,
            PlayerId = playerId,
            Success = success,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a notification message
    /// </summary>
    public static ServerMessage CreateNotification(string playerId, string message)
    {
        return new ServerMessage
        {
            Type = MessageType.Notification,
            PlayerId = playerId,
            Success = true,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create an error message
    /// </summary>
    public static ServerMessage CreateError(string playerId, string message)
    {
        return new ServerMessage
        {
            Type = MessageType.Error,
            PlayerId = playerId,
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Create a chat message to broadcast
    /// </summary>
    public static ServerMessage CreateChatMessage(string playerId, string playerName, string message, string scope = "room")
    {
        var msg = new ServerMessage
        {
            Type = MessageType.Chat,
            Success = true,
            Timestamp = DateTime.UtcNow
        };

        var data = new
        {
            playerId,
            playerName,
            message,
            scope,
            timestamp = DateTime.UtcNow
        };

        msg.Data = JsonSerializer.SerializeToElement(data, JsonOptions);
        return msg;
    }
}
