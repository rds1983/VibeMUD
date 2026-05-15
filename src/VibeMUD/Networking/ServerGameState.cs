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
