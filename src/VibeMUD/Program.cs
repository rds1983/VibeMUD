using VibeMUD.Commands;
using VibeMUD.Config;
using VibeMUD.Core;
using VibeMUD.Data;
using VibeMUD.Networking;

// Content folder is in the same directory as the executable
var contentPath = Path.Combine(AppContext.BaseDirectory, "content");

if (!Directory.Exists(contentPath))
{
    Console.WriteLine("[ERROR] Content folder not found!");
    Console.WriteLine($"[ERROR] Expected at: {contentPath}");
    Environment.Exit(1);
}

Console.WriteLine($"[Startup] Content folder: {contentPath}");

// Load configuration
var config = ConfigLoader.LoadConfig(contentPath);
Console.WriteLine($"[Startup] Loaded configuration from config.json");
Console.WriteLine($"[Startup] Server configured to run on port {config.Server.Port}");

// Display splash screen
try
{
    var splashPath = Path.Combine(contentPath, config.Display.SplashScreenFile);
    if (File.Exists(splashPath))
    {
        var splash = File.ReadAllText(splashPath);
        Console.WriteLine(splash);
    }
    else
    {
        Console.WriteLine($"[Startup] Warning: Splash screen not found at {splashPath}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Startup] Warning: Failed to load splash screen: {ex.Message}");
};

// Load game content
var gameState = new GameState();
Console.WriteLine("[Startup] Loading game data...");

try
{
    var loader = new JsonDataLoader(contentPath);
    gameState.Areas = loader.LoadAreas();
    gameState.Items = loader.LoadItems();
    gameState.NPCs = loader.LoadNPCs();
    gameState.Skills = loader.LoadSkills();
    Console.WriteLine($"[Startup] Loaded {gameState.Areas.Count} areas, {gameState.Items.Count} items, {gameState.NPCs.Count} NPCs, {gameState.Skills.Count} skills");
}
catch (Exception ex)
{
    Console.WriteLine($"[Error] Failed to load game data: {ex.Message}");
    Environment.Exit(1);
}

// Initialize command handler
var commandHandler = new CommandHandler();

// Initialize save manager for character persistence
var playerSavePath = Path.Combine(Path.GetDirectoryName(contentPath)!, "players");
if (!Directory.Exists(playerSavePath))
{
    Directory.CreateDirectory(playerSavePath);
}
var saveManager = new SaveManager(playerSavePath);
Console.WriteLine("[Startup] Character persistence path: " + playerSavePath);

// Create and start server
var server = new GameServer(gameState, commandHandler, saveManager, config);

// Load and set splash screen
try
{
    var splashPath = Path.Combine(contentPath, config.Display.SplashScreenFile);
    if (File.Exists(splashPath))
    {
        var splash = File.ReadAllText(splashPath);
        server.SetSplashScreen(splash);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Startup] Warning: Could not set splash screen: {ex.Message}");
}

// Handle shutdown gracefully
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    // Start server on configured port
    var serverTask = server.StartAsync(config.Server.Port);

    // Wait for cancellation
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("\n[Startup] Shutdown signal received");
}
finally
{
    await server.StopAsync();
    Console.WriteLine("[Startup] Server stopped");
}
