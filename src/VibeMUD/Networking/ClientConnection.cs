namespace VibeMUD.Networking;

using System.Net.Sockets;
using System.Text;
using VibeMUD.Models;

/// <summary>
/// Manages a single client connection to the game server
/// </summary>
public class ClientConnection
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly string _clientId;
    private Character? _character;
    private bool _isConnected = true;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public string ClientId => _clientId;
    public Character? Character => _character;
    public bool IsConnected => _isConnected;

    public event Func<string, Task>? OnDisconnect;
    public event Func<ClientMessage, Task>? OnMessageReceived;

    public ClientConnection(TcpClient client, string clientId)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _stream = _client.GetStream();
        _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
    }

    /// <summary>
    /// Sets the character for this connection
    /// </summary>
    public void SetCharacter(Character character)
    {
        _character = character;
    }

    /// <summary>
    /// Starts listening for messages from the client
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            while (_isConnected && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var message = await ReceiveMessageAsync();
                if (message == null)
                {
                    _isConnected = false;
                    break;
                }

                if (OnMessageReceived != null)
                {
                    await OnMessageReceived(message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientConnection] Error in {_clientId}: {ex.Message}");
            _isConnected = false;
        }
        finally
        {
            await DisconnectAsync();
        }
    }

    /// <summary>
    /// Receives a message from the client (line-based for telnet compatibility)
    /// </summary>
    private async Task<ClientMessage?> ReceiveMessageAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300)); // 5 min timeout
            using var reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);

            var line = await reader.ReadLineAsync(cts.Token);

            if (string.IsNullOrEmpty(line))
            {
                return null; // Client disconnected
            }

            // Try to parse as JSON if it starts with {
            if (line.TrimStart().StartsWith("{"))
            {
                var message = ServerProtocol.DeserializeClientMessage(line);
                return message;
            }

            // Otherwise, treat as a simple text command for telnet
            return new ClientMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Type = MessageType.Command,
                PlayerId = _character?.Id ?? "unknown",
                Command = line.Trim()
            };
        }
        catch (OperationCanceledException)
        {
            return null; // Timeout
        }
        catch (IOException)
        {
            return null; // Connection lost
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientConnection] Error receiving message in {_clientId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Sends a message to the client
    /// </summary>
    public async Task SendMessageAsync(ServerMessage message)
    {
        if (!_isConnected)
            return;

        try
        {
            var json = ServerProtocol.SerializeMessage(message);
            var bytes = Encoding.UTF8.GetBytes(json + "\n");

            await _stream.WriteAsync(bytes, 0, bytes.Length);
            await _stream.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientConnection] Error sending message to {_clientId}: {ex.Message}");
            _isConnected = false;
        }
    }

    /// <summary>
    /// Sends a raw JSON message to the client
    /// </summary>
    public async Task SendJsonAsync(string json)
    {
        if (!_isConnected)
            return;

        try
        {
            var bytes = Encoding.UTF8.GetBytes(json + "\n");
            await _stream.WriteAsync(bytes, 0, bytes.Length);
            await _stream.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientConnection] Error sending JSON to {_clientId}: {ex.Message}");
            _isConnected = false;
        }
    }

    /// <summary>
    /// Disconnects the client
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (!_isConnected)
            return;

        _isConnected = false;

        try
        {
            _stream?.Dispose();
            _client?.Close();
            _client?.Dispose();
            _cancellationTokenSource?.Cancel();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientConnection] Error disconnecting {_clientId}: {ex.Message}");
        }

        if (OnDisconnect != null)
        {
            await OnDisconnect(_clientId);
        }
    }
}
