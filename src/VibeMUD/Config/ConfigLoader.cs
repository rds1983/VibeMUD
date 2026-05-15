namespace VibeMUD.Config;

using System.Text.Json;

public class ConfigLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static ServerConfig LoadConfig(string contentPath)
    {
        var configPath = Path.Combine(contentPath, "config.json");

        if (!File.Exists(configPath))
        {
            Console.WriteLine($"[ConfigLoader] Warning: config.json not found at {configPath}, using defaults");
            return new ServerConfig();
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<ServerConfig>(json, JsonOptions);
            return config ?? new ServerConfig();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ConfigLoader] Error loading config.json: {ex.Message}, using defaults");
            return new ServerConfig();
        }
    }
}
