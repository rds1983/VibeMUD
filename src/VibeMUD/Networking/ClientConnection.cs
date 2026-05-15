namespace VibeMUD.Networking;

using System.Net.Sockets;
using System.Text;
using VibeMUD.Models;

/// <summary>
/// Manages a single client connection to the game server (plain text only)
/// </summary>
public class ClientConnection
{
    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly string _clientId;
    private Character? _character;
    private bool _isConnected = true;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public string ClientId => _clientId;
    public Character? Character => _character;
    public bool IsConnected => _isConnected;

    public event Func<string, Task>? OnDisconnect;
    public event Func<string, Task>? OnCommandReceived;

    public ClientConnection(TcpClient client, string clientId)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _reader = new StreamReader(_client.GetStream(), Encoding.UTF8);
        _writer = new StreamWriter(_client.GetStream(), Encoding.UTF8) { AutoFlush = true };
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
    /// Starts listening for commands from the client
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            while (_isConnected && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var command = await ReceiveCommandAsync();
                if (command == null)
                {
                    _isConnected = false;
                    break;
                }

                if (OnCommandReceived != null)
                {
                    await OnCommandReceived(command);
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
    /// Receives a command line from the client
    /// </summary>
    private async Task<string?> ReceiveCommandAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
            var line = await _reader.ReadLineAsync(cts.Token);
            return string.IsNullOrEmpty(line) ? null : line.Trim();
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientConnection] Error receiving command in {_clientId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Sends text to the client
    /// </summary>
    public async Task SendAsync(string text)
    {
        if (!_isConnected)
            return;

        try
        {
            await _writer.WriteLineAsync(text);
            await _writer.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientConnection] Error sending to {_clientId}: {ex.Message}");
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
            _writer?.Dispose();
            _reader?.Dispose();
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
