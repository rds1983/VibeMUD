using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Data;
using VibeMUD.Networking;

// Initialize game
Console.WriteLine("VibeMUD Server v0.7.3");
Console.WriteLine("===================");

// Load game content
var gameState = new GameState();
Console.WriteLine("[Startup] Loading game data...");

try
{
    var contentPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "content");
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

// Create and start server
var server = new GameServer(gameState, commandHandler);

// Handle shutdown gracefully
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    // Start server on port 9999
    var serverTask = server.StartAsync(9999);

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
