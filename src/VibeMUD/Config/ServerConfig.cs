namespace VibeMUD.Config;

using System.Text.Json.Serialization;

public class ServerConfig
{
    [JsonPropertyName("server")]
    public ServerSettings Server { get; set; } = new();

    [JsonPropertyName("game")]
    public GameSettings Game { get; set; } = new();

    [JsonPropertyName("display")]
    public DisplaySettings Display { get; set; } = new();

    [JsonPropertyName("features")]
    public FeatureSettings Features { get; set; } = new();

    [JsonPropertyName("logging")]
    public LoggingSettings Logging { get; set; } = new();
}

public class ServerSettings
{
    [JsonPropertyName("port")]
    public int Port { get; set; } = 9999;

    [JsonPropertyName("maxPlayers")]
    public int MaxPlayers { get; set; } = 100;

    [JsonPropertyName("idleTimeoutSeconds")]
    public int IdleTimeoutSeconds { get; set; } = 300;

    [JsonPropertyName("maxPasswordAttempts")]
    public int MaxPasswordAttempts { get; set; } = 3;
}

public class GameSettings
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "0.7.3";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "VibeMUD";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "A Multi-User Dungeon Experience";

    [JsonPropertyName("startingAreaId")]
    public string StartingAreaId { get; set; } = "starter_city";

    [JsonPropertyName("startingRoomId")]
    public string StartingRoomId { get; set; } = "city_center";
}

public class DisplaySettings
{
    [JsonPropertyName("showSplashScreen")]
    public bool ShowSplashScreen { get; set; } = true;

    [JsonPropertyName("splashScreenFile")]
    public string SplashScreenFile { get; set; } = "Splash.txt";

    [JsonPropertyName("colorSupport")]
    public bool ColorSupport { get; set; } = true;
}

public class FeatureSettings
{
    [JsonPropertyName("enableCombat")]
    public bool EnableCombat { get; set; } = true;

    [JsonPropertyName("enableShops")]
    public bool EnableShops { get; set; } = true;

    [JsonPropertyName("enableLooting")]
    public bool EnableLooting { get; set; } = true;

    [JsonPropertyName("enableNPCBehavior")]
    public bool EnableNPCBehavior { get; set; } = true;
}

public class LoggingSettings
{
    [JsonPropertyName("logServerEvents")]
    public bool LogServerEvents { get; set; } = true;

    [JsonPropertyName("logLevel")]
    public string LogLevel { get; set; } = "info";
}
