namespace VibeMUD.Networking;

using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Thread-safe async wrapper around GameState for multiplayer server
/// </summary>
public class ServerGameState
{
    private readonly GameState _gameState;
    private readonly CommandHandler _commandHandler;
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, ClientConnection> _playerConnections = new();

    public ServerGameState(GameState gameState, CommandHandler commandHandler)
    {
        _gameState = gameState;
        _commandHandler = commandHandler;
    }

    /// <summary>
    /// Registers a client connection for a player
    /// </summary>
    public void RegisterPlayerConnection(string playerId, ClientConnection connection)
    {
        _lock.EnterWriteLock();
        try
        {
            _playerConnections[playerId] = connection;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Unregisters a client connection
    /// </summary>
    public void UnregisterPlayerConnection(string playerId)
    {
        _lock.EnterWriteLock();
        try
        {
            _playerConnections.Remove(playerId);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets a client connection for a player
    /// </summary>
    public ClientConnection? GetPlayerConnection(string playerId)
    {
        _lock.EnterReadLock();
        try
        {
            _playerConnections.TryGetValue(playerId, out var connection);
            return connection;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets all connected players
    /// </summary>
    public List<string> GetConnectedPlayers()
    {
        _lock.EnterReadLock();
        try
        {
            return _playerConnections.Keys.ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Executes a command for a player (thread-safe)
    /// </summary>
    public async Task<CommandResult> ExecuteCommandAsync(string playerId, string commandInput)
    {
        _lock.EnterReadLock();
        try
        {
            var character = _gameState.GetCharacter(playerId);
            if (character == null)
                return CommandResult.Error("Character not found");

            var result = _commandHandler.ExecuteCommand(character, _gameState, commandInput);
            return await Task.FromResult(result);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Adds a character to the game
    /// </summary>
    public async Task AddCharacterAsync(Character character)
    {
        _lock.EnterWriteLock();
        try
        {
            _gameState.AddCharacter(character);
            await Task.CompletedTask;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Removes a character from the game
    /// </summary>
    public async Task RemoveCharacterAsync(string characterId)
    {
        _lock.EnterWriteLock();
        try
        {
            _gameState.RemoveCharacter(characterId);
            await Task.CompletedTask;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets a character
    /// </summary>
    public Character? GetCharacter(string characterId)
    {
        _lock.EnterReadLock();
        try
        {
            return _gameState.GetCharacter(characterId);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets a room
    /// </summary>
    public Room? GetRoom(string areaId, string roomId)
    {
        _lock.EnterReadLock();
        try
        {
            return _gameState.GetRoom(areaId, roomId);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets all players in a room
    /// </summary>
    public List<Character> GetPlayersInRoom(string areaId, string roomId)
    {
        _lock.EnterReadLock();
        try
        {
            return _gameState.GetPlayersInRoom(areaId, roomId);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets all NPCs in a room
    /// </summary>
    public List<NPC> GetNPCsInRoom(string areaId, string roomId)
    {
        _lock.EnterReadLock();
        try
        {
            return _gameState.GetNPCsInRoom(areaId, roomId);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets all items in a room
    /// </summary>
    public List<Item> GetItemsInRoom(string areaId, string roomId)
    {
        _lock.EnterReadLock();
        try
        {
            return _gameState.GetItemsInRoom(areaId, roomId);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Creates a room state update for a player
    /// </summary>
    public RoomStateUpdate CreateRoomStateUpdate(Character character)
    {
        _lock.EnterReadLock();
        try
        {
            var room = _gameState.GetRoom(character.CurrentAreaId!, character.CurrentRoomId!);
            if (room == null)
                return new RoomStateUpdate();

            var update = new RoomStateUpdate
            {
                AreaId = character.CurrentAreaId!,
                RoomId = character.CurrentRoomId!,
                RoomTitle = room.Title,
                RoomDescription = room.Description
            };

            // Add players
            var players = _gameState.GetPlayersInRoom(character.CurrentAreaId!, character.CurrentRoomId!);
            foreach (var player in players.Where(p => p.Id != character.Id))
            {
                update.PlayersInRoom.Add(new PlayerInfo
                {
                    Id = player.Id,
                    Name = player.Name,
                    Class = player.Class,
                    Level = player.Level,
                    Health = player.Health,
                    MaxHealth = player.MaxHealth
                });
            }

            // Add NPCs
            var npcs = _gameState.GetNPCsInRoom(character.CurrentAreaId!, character.CurrentRoomId!);
            foreach (var npc in npcs)
            {
                update.NPCsInRoom.Add(new NPCInfo
                {
                    Id = npc.Id,
                    Name = npc.Name,
                    Level = npc.Level,
                    Health = npc.Health,
                    MaxHealth = npc.MaxHealth
                });
            }

            // Add items
            var items = _gameState.GetItemsInRoom(character.CurrentAreaId!, character.CurrentRoomId!);
            foreach (var item in items)
            {
                update.ItemsInRoom.Add(new ItemInfo
                {
                    Id = item.Id,
                    Name = item.Name,
                    Type = item.Type
                });
            }

            // Add exits
            foreach (var dir in new[] { "north", "south", "east", "west", "up", "down" })
            {
                if (room.HasExit(dir))
                {
                    var exit = room.GetExit(dir);
                    if (exit.HasValue)
                    {
                        var (nextAreaId, nextRoomId) = exit.Value;
                        update.Exits[dir] = $"{nextAreaId}/{nextRoomId}";
                    }
                }
            }

            return update;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Notifies all players in a room about a message
    /// </summary>
    public async Task NotifyPlayersInRoomAsync(string areaId, string roomId, ServerMessage message)
    {
        _lock.EnterReadLock();
        try
        {
            var players = _gameState.GetPlayersInRoom(areaId, roomId);
            var tasks = new List<Task>();

            foreach (var player in players)
            {
                if (_playerConnections.TryGetValue(player.Id, out var connection))
                {
                    tasks.Add(connection.SendMessageAsync(message));
                }
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Broadcasts room state to all players in a room
    /// </summary>
    public async Task BroadcastRoomStateAsync(string areaId, string roomId)
    {
        _lock.EnterReadLock();
        try
        {
            var players = _gameState.GetPlayersInRoom(areaId, roomId);
            var tasks = new List<Task>();

            foreach (var player in players)
            {
                if (_playerConnections.TryGetValue(player.Id, out var connection))
                {
                    var update = CreateRoomStateUpdate(player);
                    var message = new ServerMessage
                    {
                        Type = MessageType.RoomState,
                        PlayerId = player.Id,
                        Success = true,
                        Message = "Room state update"
                    };

                    message.Data = System.Text.Json.JsonSerializer.SerializeToElement(update, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });

                    tasks.Add(connection.SendMessageAsync(message));
                }
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets the underlying GameState (for direct access when needed)
    /// </summary>
    public GameState GetGameState()
    {
        return _gameState;
    }

    /// <summary>
    /// Disposes the lock
    /// </summary>
    public void Dispose()
    {
        _lock?.Dispose();
    }
}
