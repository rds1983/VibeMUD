namespace VibeMUD.Networking;

using System.Net;
using System.Net.Sockets;
using System.Text;
using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Data;
using VibeMUD.Models;

/// <summary>
/// Main game server that handles TCP connections and game state (plain text protocol)
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
            Console.WriteLine("[GameServer] Connect with: telnet localhost 9999");

            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
                catch (ObjectDisposedException)
                {
                    break;
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
            connection.OnCommandReceived += async (command) => await HandleCommandAsync(clientId, connection, command);
            connection.OnDisconnect += async (id) => await HandleClientDisconnectAsync(id);

            _clientConnections[clientId] = connection;

            // Send welcome message
            await connection.SendAsync("");
            await connection.SendAsync("=== WELCOME TO VIBEMUD ===");
            await connection.SendAsync("");
            await connection.SendAsync("Type 'create <name> <class>' to create a character");
            await connection.SendAsync("Example: create Aragorn warrior");
            await connection.SendAsync("Classes: warrior, thief, mage, cleric, monk, druid");
            await connection.SendAsync("");
            await connection.SendAsync("> ");

            // Start listening for commands from this client
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
    /// Handles a command from a client
    /// </summary>
    private async Task HandleCommandAsync(string clientId, ClientConnection connection, string commandLine)
    {
        try
        {
            var character = connection.Character;

            // Handle character creation
            if (character == null && commandLine.StartsWith("create ", StringComparison.OrdinalIgnoreCase))
            {
                await HandleCharacterCreationAsync(clientId, connection, commandLine);
                return;
            }

            // Check if authenticated
            if (character == null)
            {
                await connection.SendAsync("[ERROR] You must create a character first. Type: create <name> <class>");
                await connection.SendAsync("> ");
                return;
            }

            // Execute game command
            var result = _commandHandler.ExecuteCommand(character, _gameState, commandLine);

            // Send response
            if (result.Success)
            {
                await connection.SendAsync(result.Message);
            }
            else
            {
                await connection.SendAsync($"[ERROR] {result.Message}");
            }

            // Send room state if player moved
            var updatedChar = _serverGameState.GetCharacter(character.Id);
            if (updatedChar != null && (updatedChar.CurrentAreaId != character.CurrentAreaId || updatedChar.CurrentRoomId != character.CurrentRoomId))
            {
                connection.SetCharacter(updatedChar);
                await BroadcastRoomStateAsync(updatedChar.CurrentAreaId!, updatedChar.CurrentRoomId!);
            }
            else if (updatedChar != null)
            {
                connection.SetCharacter(updatedChar);
            }

            await connection.SendAsync("> ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error executing command: {ex.Message}");
            await connection.SendAsync($"[ERROR] {ex.Message}");
            await connection.SendAsync("> ");
        }
    }

    /// <summary>
    /// Handles character creation
    /// </summary>
    private async Task HandleCharacterCreationAsync(string clientId, ClientConnection connection, string commandLine)
    {
        try
        {
            var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                await connection.SendAsync("[ERROR] Usage: create <name> <class>");
                await connection.SendAsync("> ");
                return;
            }

            var playerName = parts[1];
            var playerClass = parts[2].ToLower();

            var validClasses = new[] { "warrior", "thief", "mage", "cleric", "monk", "druid" };
            if (!validClasses.Contains(playerClass))
            {
                await connection.SendAsync("[ERROR] Invalid class. Choose: warrior, thief, mage, cleric, monk, druid");
                await connection.SendAsync("> ");
                return;
            }

            var playerId = clientId;
            var character = new Character(playerId, playerName, playerClass)
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

            connection.SetCharacter(character);
            _serverGameState.RegisterPlayerConnection(playerId, connection);

            await connection.SendAsync($"\nWelcome, {playerName}! You are in the game.\n");
            await BroadcastRoomStateAsync("starter_city", "city_center");
            await connection.SendAsync("> ");

            Console.WriteLine($"[GameServer] Player {playerName} ({playerId}) created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error in character creation: {ex.Message}");
            await connection.SendAsync($"[ERROR] {ex.Message}");
            await connection.SendAsync("> ");
        }
    }

    /// <summary>
    /// Broadcasts room state to all players in a room
    /// </summary>
    private async Task BroadcastRoomStateAsync(string areaId, string roomId)
    {
        try
        {
            var players = _serverGameState.GetPlayersInRoom(areaId, roomId);
            var room = _serverGameState.GetRoom(areaId, roomId);

            if (room == null)
                return;

            var roomInfo = new StringBuilder();
            roomInfo.AppendLine("");
            roomInfo.AppendLine($"=== {room.Title} ===");
            roomInfo.AppendLine(room.Description);
            roomInfo.AppendLine("");

            // Show NPCs
            var npcs = _serverGameState.GetNPCsInRoom(areaId, roomId);
            if (npcs.Any())
            {
                roomInfo.AppendLine("NPCs here:");
                foreach (var npc in npcs)
                {
                    roomInfo.AppendLine($"  {npc.Name} (Level {npc.Level})");
                }
                roomInfo.AppendLine("");
            }

            // Show other players
            var otherPlayers = players.Where(p => _serverGameState.GetPlayerConnection(p.Id) != null).ToList();
            if (otherPlayers.Any())
            {
                roomInfo.AppendLine("Players here:");
                foreach (var player in otherPlayers)
                {
                    roomInfo.AppendLine($"  {player.Name} (Level {player.Level} {player.Class})");
                }
                roomInfo.AppendLine("");
            }

            // Show exits
            var exits = new List<string>();
            foreach (var dir in new[] { "north", "south", "east", "west", "up", "down" })
            {
                if (room.HasExit(dir))
                    exits.Add(dir.Substring(0, 1).ToUpper() + dir.Substring(1));
            }
            if (exits.Any())
            {
                roomInfo.AppendLine($"Exits: {string.Join(", ", exits)}");
            }

            var output = roomInfo.ToString();

            // Send to all players in room
            foreach (var player in players)
            {
                var conn = _serverGameState.GetPlayerConnection(player.Id);
                if (conn != null)
                {
                    await conn.SendAsync(output);
                }
            }
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

                    // Remove from room
                    if (!string.IsNullOrEmpty(character.CurrentAreaId) && !string.IsNullOrEmpty(character.CurrentRoomId))
                    {
                        var room = _serverGameState.GetRoom(character.CurrentAreaId, character.CurrentRoomId);
                        room?.RemovePlayer(character.Id);

                        // Notify others in room
                        await BroadcastRoomStateAsync(character.CurrentAreaId, character.CurrentRoomId);
                    }

                    // Remove from game
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
