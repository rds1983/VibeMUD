namespace VibeMUD.Networking;

using System.Net;
using System.Net.Sockets;
using System.Text;
using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Data;
using VibeMUD.Models;
using VibeMUD.Utilities;

/// <summary>
/// Main game server that handles TCP connections and game state (plain text protocol)
/// </summary>
public class GameServer
{
    private readonly GameState _gameState;
    private readonly CommandHandler _commandHandler;
    private readonly ServerGameState _serverGameState;
    private readonly SaveManager _saveManager;
    private readonly PasswordHasher _passwordHasher = new();
    private TcpListener? _listener;
    private bool _isRunning;
    private int _port = 9999;
    private string _splashScreen = string.Empty;
    private readonly Dictionary<string, ClientConnection> _clientConnections = new();

    public GameServer(GameState gameState, CommandHandler commandHandler, SaveManager saveManager)
    {
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _saveManager = saveManager ?? throw new ArgumentNullException(nameof(saveManager));
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
            // Load splash screen
            try
            {
                var splashPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "content", "Splash.txt");
                if (File.Exists(splashPath))
                {
                    _splashScreen = File.ReadAllText(splashPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameServer] Warning: Could not load splash screen: {ex.Message}");
            }

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

            // Send splash screen
            if (!string.IsNullOrEmpty(_splashScreen))
            {
                await connection.SendAsync(_splashScreen);
            }
            else
            {
                // Fallback if splash screen failed to load
                await connection.SendAsync("");
                await connection.SendAsync("=== WELCOME TO VIBEMUD ===");
                await connection.SendAsync("");
            }

            // Send login prompt
            await connection.SendAsync("Please enter your character name:");
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
            // Route based on login state
            if (connection.LoginState != LoginState.Authenticated)
            {
                await HandleLoginFlowAsync(clientId, connection, commandLine);
                return;
            }

            var character = connection.Character;
            if (character == null)
            {
                await connection.SendAsync("[ERROR] Authentication lost. Disconnecting.");
                await connection.DisconnectAsync();
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

            // Save character periodically (on any command that might change state)
            _saveManager.SaveCharacter(character);

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
    /// Routes login flow based on current state
    /// </summary>
    private async Task HandleLoginFlowAsync(string clientId, ClientConnection connection, string input)
    {
        try
        {
            switch (connection.LoginState)
            {
                case LoginState.AskingName:
                    await HandleAskingNameAsync(connection, input);
                    break;

                case LoginState.AskingPassword:
                    await HandleAskingPasswordAsync(connection, input);
                    break;

                case LoginState.AskingCreateConfirmation:
                    await HandleCreateConfirmationAsync(connection, input);
                    break;

                case LoginState.AskingNewPassword:
                    await HandleNewPasswordAsync(connection, input);
                    break;

                case LoginState.ConfirmingPassword:
                    await HandleConfirmPasswordAsync(connection, input);
                    break;

                case LoginState.AskingClass:
                    await HandleClassSelectionAsync(clientId, connection, input);
                    break;

                default:
                    await connection.SendAsync("[ERROR] Unknown login state");
                    await connection.DisconnectAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameServer] Error in login flow: {ex.Message}");
            await connection.SendAsync($"[ERROR] {ex.Message}");
            await connection.DisconnectAsync();
        }
    }

    private async Task HandleAskingNameAsync(ClientConnection connection, string input)
    {
        var characterName = input.Trim();

        if (string.IsNullOrWhiteSpace(characterName) || characterName.Length < 3 || characterName.Length > 20)
        {
            await connection.SendAsync("[ERROR] Character name must be 3-20 characters");
            await connection.SendAsync("Please enter your character name:");
            await connection.SendAsync("> ");
            return;
        }

        connection.SetAttemptedCharacterName(characterName);

        if (_saveManager.CharacterExistsByName(characterName))
        {
            await connection.SendAsync($"Welcome back, {characterName}!");
            await connection.SendAsync("Please enter your password:");
            connection.SetLoginState(LoginState.AskingPassword);
            await connection.SendAsync("> ");
        }
        else
        {
            await connection.SendAsync($"No character named '{characterName}' found.");
            await connection.SendAsync("Would you like to create a new character? (yes/no)");
            connection.SetLoginState(LoginState.AskingCreateConfirmation);
            await connection.SendAsync("> ");
        }
    }

    private async Task HandleAskingPasswordAsync(ClientConnection connection, string input)
    {
        var password = input;
        var characterName = connection.AttemptedCharacterName;
        var character = _saveManager.LoadCharacterByName(characterName);

        if (character == null)
        {
            await connection.SendAsync("[ERROR] Character not found");
            connection.ResetLoginState();
            await connection.SendAsync("Please enter your character name:");
            await connection.SendAsync("> ");
            return;
        }

        if (!_passwordHasher.VerifyPassword(password, character.PasswordHash))
        {
            connection.IncrementWrongPasswordAttempts();

            if (connection.WrongPasswordAttempts >= 3)
            {
                await connection.SendAsync("[ERROR] Too many wrong password attempts. Disconnecting.");
                await connection.DisconnectAsync();
                return;
            }

            await connection.SendAsync($"[ERROR] Wrong password. ({3 - connection.WrongPasswordAttempts} attempts remaining)");
            await connection.SendAsync("Please enter your password:");
            await connection.SendAsync("> ");
            return;
        }

        // Password correct - load character into game
        await LoadCharacterIntoGameAsync(character, connection);
    }

    private async Task HandleCreateConfirmationAsync(ClientConnection connection, string input)
    {
        var response = input.Trim().ToLower();

        if (response == "yes")
        {
            await connection.SendAsync("Please enter a password for your new character:");
            connection.SetLoginState(LoginState.AskingNewPassword);
            await connection.SendAsync("> ");
        }
        else if (response == "no")
        {
            connection.ResetLoginState();
            await connection.SendAsync("Please enter your character name:");
            await connection.SendAsync("> ");
        }
        else
        {
            await connection.SendAsync("Please answer 'yes' or 'no'");
            await connection.SendAsync("> ");
        }
    }

    private async Task HandleNewPasswordAsync(ClientConnection connection, string input)
    {
        var password = input;

        if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
        {
            await connection.SendAsync("[ERROR] Password must be at least 4 characters");
            await connection.SendAsync("Please enter a password for your new character:");
            await connection.SendAsync("> ");
            return;
        }

        connection.SetPasswordAttempt(password);
        await connection.SendAsync("Please confirm your password:");
        connection.SetLoginState(LoginState.ConfirmingPassword);
        await connection.SendAsync("> ");
    }

    private async Task HandleConfirmPasswordAsync(ClientConnection connection, string input)
    {
        var confirmPassword = input;
        var originalPassword = connection.PasswordAttempt;

        if (confirmPassword != originalPassword)
        {
            await connection.SendAsync("[ERROR] Passwords do not match");
            await connection.SendAsync("Please enter a password for your new character:");
            connection.SetLoginState(LoginState.AskingNewPassword);
            connection.SetPasswordAttempt(string.Empty);
            await connection.SendAsync("> ");
            return;
        }

        await connection.SendAsync("Select your class:");
        await connection.SendAsync("  warrior, thief, mage, cleric, monk, druid");
        connection.SetLoginState(LoginState.AskingClass);
        await connection.SendAsync("> ");
    }

    private async Task HandleClassSelectionAsync(string clientId, ClientConnection connection, string input)
    {
        var playerClass = input.Trim().ToLower();
        var validClasses = new[] { "warrior", "thief", "mage", "cleric", "monk", "druid" };

        if (!validClasses.Contains(playerClass))
        {
            await connection.SendAsync("[ERROR] Invalid class. Choose: warrior, thief, mage, cleric, monk, druid");
            await connection.SendAsync("> ");
            return;
        }

        // Create the character
        var playerName = connection.AttemptedCharacterName;
        var password = connection.PasswordAttempt;
        var passwordHash = _passwordHasher.HashPassword(password);

        var character = new Character(clientId, playerName, playerClass)
        {
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
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

        // Save character to disk
        _saveManager.SaveCharacter(character);

        // Add to game
        await _serverGameState.AddCharacterAsync(character);
        var room = _serverGameState.GetRoom("starter_city", "city_center");
        if (room != null)
        {
            room.AddPlayer(clientId);
        }

        // Load into connection
        await LoadCharacterIntoGameAsync(character, connection);

        Console.WriteLine($"[GameServer] Player {playerName} ({clientId}) created");
    }

    private async Task LoadCharacterIntoGameAsync(Character character, ClientConnection connection)
    {
        connection.SetCharacter(character);
        _serverGameState.RegisterPlayerConnection(character.Id, connection);

        await connection.SendAsync($"\nWelcome, {character.Name}! You are now in the game.\n");
        await BroadcastRoomStateAsync(character.CurrentAreaId!, character.CurrentRoomId!);
        await connection.SendAsync("> ");
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

                    // Save character to disk before removing
                    _saveManager.SaveCharacter(character);

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
