namespace VibeMUD.Commands;

using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Moves a character in a direction
/// </summary>
public class MoveCommand : Command
{
    private static readonly Dictionary<string, string> DirectionMap = new()
    {
        ["north"] = "north",
        ["n"] = "north",
        ["south"] = "south",
        ["s"] = "south",
        ["east"] = "east",
        ["e"] = "east",
        ["west"] = "west",
        ["w"] = "west",
        ["up"] = "up",
        ["u"] = "up",
        ["down"] = "down",
        ["d"] = "down"
    };

    public MoveCommand()
    {
        Name = "move";
        Description = "Move in a direction";
        Aliases = new() { "north", "south", "east", "west", "up", "down", "n", "s", "e", "w", "u", "d" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        // The direction is passed as the command name by CommandHandler
        // We need to look at the actual command that was called
        return CommandResult.Error("Use directional commands: north, south, east, west, up, down");
    }

    public static CommandResult Move(Character character, GameState gameState, string direction)
    {
        if (!DirectionMap.TryGetValue(direction.ToLower(), out var normalizedDirection))
            return CommandResult.Error($"Invalid direction: {direction}");

        if (string.IsNullOrEmpty(character.CurrentAreaId) || string.IsNullOrEmpty(character.CurrentRoomId))
            return CommandResult.Error("You are not in a valid location.");

        var room = gameState.GetRoom(character.CurrentAreaId, character.CurrentRoomId);
        if (room == null)
            return CommandResult.Error("The room no longer exists.");

        if (!room.HasExit(normalizedDirection))
            return CommandResult.Error($"You cannot go {normalizedDirection} from here.");

        var exit = room.GetExit(normalizedDirection);
        if (exit == null)
            return CommandResult.Error($"Exit {normalizedDirection} is blocked.");

        var (nextAreaId, nextRoomId) = exit.Value;
        var nextRoom = gameState.GetRoom(nextAreaId, nextRoomId);
        if (nextRoom == null)
            return CommandResult.Error("That exit leads nowhere.");

        // Remove from current room
        room.RemovePlayer(character.Id);

        // Update character location
        character.CurrentAreaId = nextAreaId;
        character.CurrentRoomId = nextRoomId;

        // Add to new room
        nextRoom.AddPlayer(character.Id);

        var message = $"You go {normalizedDirection}.\n\n=== {nextRoom.Title} ===\n{nextRoom.Description}\n\n{nextRoom.GetExitDescription()}";
        return CommandResult.Ok(message);
    }
}

/// <summary>
/// Specialized commands for each direction
/// </summary>
public class NorthCommand : Command
{
    public NorthCommand()
    {
        Name = "north";
        Aliases = new() { "n" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
        => MoveCommand.Move(character, gameState, "north");
}

public class SouthCommand : Command
{
    public SouthCommand()
    {
        Name = "south";
        Aliases = new() { "s" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
        => MoveCommand.Move(character, gameState, "south");
}

public class EastCommand : Command
{
    public EastCommand()
    {
        Name = "east";
        Aliases = new() { "e" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
        => MoveCommand.Move(character, gameState, "east");
}

public class WestCommand : Command
{
    public WestCommand()
    {
        Name = "west";
        Aliases = new() { "w" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
        => MoveCommand.Move(character, gameState, "west");
}

public class UpCommand : Command
{
    public UpCommand()
    {
        Name = "up";
        Aliases = new() { "u" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
        => MoveCommand.Move(character, gameState, "up");
}

public class DownCommand : Command
{
    public DownCommand()
    {
        Name = "down";
        Aliases = new() { "d" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
        => MoveCommand.Move(character, gameState, "down");
}
