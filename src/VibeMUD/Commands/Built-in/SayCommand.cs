namespace VibeMUD.Commands.Built_in;

using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Say something in the local room (visible to all players in the room)
/// </summary>
public class SayCommand : Command
{
    public SayCommand()
    {
        Name = "say";
        Description = "Say something in the room: say <message>";
        Aliases = new() { "s" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Error("Say what?");

        var message = string.Join(" ", args);
        var output = $"{character.Name} says: {message}";

        var room = gameState.GetRoom(character.CurrentAreaId!, character.CurrentRoomId!);
        if (room == null)
            return CommandResult.Error("You are not in a valid room.");

        return CommandResult.Ok(output);
    }

    public override bool ValidateArgs(string[] args) => true;
}
