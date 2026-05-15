namespace VibeMUD.Commands.Built_in;

using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Send a global chat message visible to all players
/// </summary>
public class ChatCommand : Command
{
    public ChatCommand()
    {
        Name = "chat";
        Description = "Send a global chat message: chat <message>";
        Aliases = new() { "c" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Error("Say what?");

        var message = string.Join(" ", args);
        var output = $"[GLOBAL] {character.Name}: {message}";
        return CommandResult.Ok(output);
    }

    public override bool ValidateArgs(string[] args) => true;
}
