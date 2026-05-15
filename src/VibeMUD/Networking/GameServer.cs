namespace VibeMUD.Networking;

using System.Net;
using System.Net.Sockets;
using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Data;
using VibeMUD.Models;

/// <summary>
/// Main game server that handles TCP connections and game state
/// </summary>
public class GameServer
{
    private readonly GameState _gameState;
    private readonly CommandHandler _commandHandler;
    private readonly ServerGameState _serverGameState;
    private TcpListener? _listener;
    private bool _isRunning;
    private int _port = 9999;
    private readonly Dictionary<string, ClientConnection> _clientConnections = new();

    public GameServer(GameState gameState, CommandHandler commandHandler)
    {
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _serverGameState = new ServerGameState(_gameState, _commandHandler);
    }

    /// <summary>
    /// Starts the server
    /// </summary>
    public async Task StartAsync(int port = 9999)
    {
        if (_isRunning)
            throw new InvalidOperationException("Server is already running");

        _port = port;
        _isRunning = true;

        try
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Console.WriteLine($"[GameServer] Server started on port {_port}");

            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
                catch (ObjectDisposedException)
                {
                    break; // Server stopped
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GameServer] Error accepting client: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Fatal error: {ex.Message}");
            _isRunning = false;
        }
    }

    /// <summary>
    /// Handles a new client connection
    /// </summary>
    private async Task HandleClientAsync(TcpClient client)
    {
        var clientId = Guid.NewGuid().ToString();
        var connection = new ClientConnection(client, clientId);

        Console.WriteLine($"[GameServer] New connection: {clientId}");

        try
        {
            // Expect first message to be player login/creation
            connection.OnMessageReceived += async (message) => await HandleClientMessageAsync(clientId, connection, message);
            connection.OnDisconnect += async (id) => await HandleClientDisconnectAsync(id);

            _clientConnections[clientId] = connection;
            _serverGameState.RegisterPlayerConnection(clientId, connection);

            // Send welcome message (telnet-friendly)
            await connection.SendJsonAsync(@"
=== WELCOME TO VIBEMUD ===

To login, enter your character as JSON:
{
  ""type"": ""connect"",
  ""playerId"": ""your-id"",
  ""playerName"": ""YourName"",
  ""characterClass"": ""warrior""
}

Or start with a simple command!
Type 'help' for commands.

> ");

            // Start listening for messages from this client
            await connection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error handling client {clientId}: {ex.Message}");
        }
        finally
        {
            _clientConnections.Remove(clientId);
            _serverGameState.UnregisterPlayerConnection(clientId);
        }
    }

    /// <summary>
    /// Handles a message from a client
    /// </summary>
    private async Task HandleClientMessageAsync(string clientId, ClientConnection connection, ClientMessage message)
    {
        try
        {
            switch (message.Type)
            {
                case MessageType.Connect:
                    await HandlePlayerConnectAsync(clientId, connection, message);
                    break;

                case MessageType.Command:
                    await HandleCommandAsync(clientId, connection, message);
                    break;

                case MessageType.Chat:
                    await HandleChatAsync(clientId, connection, message);
                    break;

                default:
                    var error = ServerProtocol.CreateError(clientId, $"Unknown message type: {message.Type}");
                    await connection.SendMessageAsync(error);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error handling message from {clientId}: {ex.Message}");
            var error = ServerProtocol.CreateError(clientId, "Internal server error");
            await connection.SendMessageAsync(error);
        }
    }

    /// <summary>
    /// Handles player connection/login
    /// </summary>
    private async Task HandlePlayerConnectAsync(string clientId, ClientConnection connection, ClientMessage message)
    {
        var playerId = message.PlayerId;
        var playerName = message.PlayerName;
        var playerClass = message.CharacterClass;

        if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(playerName))
        {
            await connection.SendJsonAsync("[ERROR] Invalid player data. Please provide playerId and playerName.\n\n> ");
            return;
        }

        try
        {
            // Check if character exists
            var existingChar = _serverGameState.GetCharacter(playerId);

            Character character;
            if (existingChar != null)
            {
                // Load existing character
                character = existingChar;
                Console.WriteLine($"[GameServer] Player {playerName} ({playerId}) logged in");
            }
            else
            {
                // Create new character
                character = new Character(playerId, playerName, playerClass ?? "warrior")
                {
                    Health = 100,
                    MaxHealth = 100,
                    Mana = 50,
                    MaxMana = 50,
                    Level = 1,
                    Strength = 10,
                    Dexterity = 10,
                    Constitution = 10,
                    Intelligence = 10,
                    Wisdom = 10,
                    Charisma = 10,
                    CurrentAreaId = "starter_city",
                    CurrentRoomId = "city_center"
                };

                await _serverGameState.AddCharacterAsync(character);
                var room = _serverGameState.GetRoom("starter_city", "city_center");
                if (room != null)
                {
                    room.AddPlayer(playerId);
                }

                Console.WriteLine($"[GameServer] New player {playerName} ({playerId}) created");
            }

            connection.SetCharacter(character);

            // Send success response (telnet-friendly)
            await connection.SendJsonAsync($"\nWelcome, {playerName}! You are in the game.\n\n> ");

            // Broadcast initial room state
            await BroadcastRoomStateAsync(character.CurrentAreaId!, character.CurrentRoomId!);

            // Notify room that player joined
            var notification = ServerProtocol.CreateNotification(string.Empty, $"{playerName} has joined the game");
            await _serverGameState.NotifyPlayersInRoomAsync(character.CurrentAreaId!, character.CurrentRoomId!, notification);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error in player connect: {ex.Message}");
            await connection.SendJsonAsync($"[ERROR] Failed to connect player: {ex.Message}\n\n> ");
        }
    }

    /// <summary>
    /// Handles a game command from a player
    /// </summary>
    private async Task HandleCommandAsync(string clientId, ClientConnection connection, ClientMessage message)
    {
        var character = connection.Character;
        if (character == null)
        {
            await connection.SendJsonAsync("[ERROR] You must be connected first\n\n> ");
            return;
        }

        if (string.IsNullOrEmpty(message.Command))
        {
            await connection.SendJsonAsync("[ERROR] No command specified\n\n> ");
            return;
        }

        try
        {
            // Build command string from command and args
            var commandStr = message.Command;
            if (message.Args != null && message.Args.Length > 0)
            {
                commandStr += " " + string.Join(" ", message.Args);
            }

            // Execute command
            var result = await _serverGameState.ExecuteCommandAsync(character.Id, commandStr);

            // Send result to player (telnet-friendly text format)
            var prefix = result.Success ? "" : "[ERROR] ";
            await connection.SendJsonAsync($"{prefix}{result.Message}\n> ");

            // Broadcast room state changes
            if (result.Success)
            {
                // Check if player moved
                var newChar = _serverGameState.GetCharacter(character.Id);
                if (newChar != null && (newChar.CurrentAreaId != character.CurrentAreaId || newChar.CurrentRoomId != character.CurrentRoomId))
                {
                    // Player moved - notify old and new rooms
                    if (!string.IsNullOrEmpty(character.CurrentAreaId) && !string.IsNullOrEmpty(character.CurrentRoomId))
                    {
                        var oldRoomNotif = ServerProtocol.CreateNotification(string.Empty, $"{character.Name} has left");
                        await _serverGameState.NotifyPlayersInRoomAsync(character.CurrentAreaId, character.CurrentRoomId, oldRoomNotif);
                    }

                    await BroadcastRoomStateAsync(newChar.CurrentAreaId!, newChar.CurrentRoomId!);

                    var newRoomNotif = ServerProtocol.CreateNotification(string.Empty, $"{newChar.Name} has arrived");
                    await _serverGameState.NotifyPlayersInRoomAsync(newChar.CurrentAreaId!, newChar.CurrentRoomId!, newRoomNotif);

                    // Update local reference
                    connection.SetCharacter(newChar);
                }
                else
                {
                    // Player stayed in same room - broadcast room state
                    await BroadcastRoomStateAsync(character.CurrentAreaId!, character.CurrentRoomId!);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error executing command: {ex.Message}");
            await connection.SendJsonAsync($"[ERROR] Error executing command: {ex.Message}\n\n> ");
        }
    }

    /// <summary>
    /// Handles chat messages from a player
    /// </summary>
    private async Task HandleChatAsync(string clientId, ClientConnection connection, ClientMessage message)
    {
        var character = connection.Character;
        if (character == null)
        {
            await connection.SendJsonAsync("[ERROR] You must be connected first\n\n> ");
            return;
        }

        var scope = message.Scope ?? "room";
        var chatText = $"[{scope.ToUpper()}] {character.Name}: {message.Message ?? ""}";

        try
        {
            switch (scope.ToLower())
            {
                case "room":
                    var roomPlayers = _serverGameState.GetPlayersInRoom(character.CurrentAreaId!, character.CurrentRoomId!);
                    foreach (var player in roomPlayers)
                    {
                        var conn = _serverGameState.GetPlayerConnection(player.Id);
                        if (conn != null)
                        {
                            await conn.SendJsonAsync($"{chatText}\n\n> ");
                        }
                    }
                    break;

                case "area":
                    var areaPlayers = _serverGameState.GetGameState().Characters.Values
                        .Where(c => c.CurrentAreaId == character.CurrentAreaId)
                        .ToList();
                    foreach (var player in areaPlayers)
                    {
                        var conn = _serverGameState.GetPlayerConnection(player.Id);
                        if (conn != null)
                        {
                            await conn.SendJsonAsync($"{chatText}\n\n> ");
                        }
                    }
                    break;

                case "global":
                    foreach (var conn in _clientConnections.Values)
                    {
                        await conn.SendJsonAsync($"{chatText}\n\n> ");
                    }
                    break;

                default:
                    await connection.SendJsonAsync("[ERROR] Invalid chat scope (room, area, or global)\n\n> ");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error handling chat: {ex.Message}");
            await connection.SendJsonAsync($"[ERROR] Error sending chat message\n\n> ");
        }
    }

    /// <summary>
    /// Broadcasts room state to all players in a room
    /// </summary>
    private async Task BroadcastRoomStateAsync(string areaId, string roomId)
    {
        try
        {
            await _serverGameState.BroadcastRoomStateAsync(areaId, roomId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error broadcasting room state: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles client disconnection
    /// </summary>
    private async Task HandleClientDisconnectAsync(string clientId)
    {
        try
        {
            if (_clientConnections.TryGetValue(clientId, out var connection))
            {
                var character = connection.Character;
                if (character != null)
                {
                    Console.WriteLine($"[GameServer] Player {character.Name} ({character.Id}) disconnected");

                    // Notify room that player left
                    if (!string.IsNullOrEmpty(character.CurrentAreaId) && !string.IsNullOrEmpty(character.CurrentRoomId))
                    {
                        var notification = ServerProtocol.CreateNotification(string.Empty, $"{character.Name} has disconnected");
                        await _serverGameState.NotifyPlayersInRoomAsync(character.CurrentAreaId, character.CurrentRoomId, notification);

                        // Remove player from room
                        var room = _serverGameState.GetRoom(character.CurrentAreaId, character.CurrentRoomId);
                        room?.RemovePlayer(character.Id);

                        // Broadcast room state change
                        await BroadcastRoomStateAsync(character.CurrentAreaId, character.CurrentRoomId);
                    }

                    // Remove character from game
                    await _serverGameState.RemoveCharacterAsync(character.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error handling disconnect: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops the server
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        Console.WriteLine("[GameServer] Shutting down server...");

        try
        {
            // Disconnect all clients
            var clients = _clientConnections.Values.ToList();
            foreach (var client in clients)
            {
                await client.DisconnectAsync();
            }

            _listener?.Stop();
            _listener?.Dispose();
            _serverGameState?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error during shutdown: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets connected player count
    /// </summary>
    public int GetConnectedPlayerCount()
    {
        return _clientConnections.Count;
    }
}
