namespace VibeMUD.Commands;

using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Base class for all game commands
/// </summary>
public abstract class Command
{
    public string Name { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public List<string> Aliases { get; protected set; } = new();

    /// <summary>
    /// Executes the command
    /// </summary>
    public abstract CommandResult Execute(Character character, GameState gameState, string[] args);

    /// <summary>
    /// Validates command usage
    /// </summary>
    public virtual bool ValidateArgs(string[] args)
    {
        return true;
    }
}

/// <summary>
/// Result of executing a command
/// </summary>
public class CommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool ShouldUpdateDisplay { get; set; } = true;

    public CommandResult(bool success, string message, bool shouldUpdateDisplay = true)
    {
        Success = success;
        Message = message;
        ShouldUpdateDisplay = shouldUpdateDisplay;
    }

    public static CommandResult Ok(string message)
        => new(true, message);

    public static CommandResult Error(string message)
        => new(false, message);

    public static CommandResult Silent(string message)
        => new(true, message, false);
}
