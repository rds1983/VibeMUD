namespace VibeMUD.Commands.Built_in;

using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Send a chat message with scope control (room, area, global)
/// </summary>
public class ChatCommand : Command
{
    public ChatCommand()
    {
        Name = "chat";
        Description = "Send a chat message: chat <scope> <message> (scope: room/area/global)";
        Aliases = new() { "c" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (args.Length < 2)
            return CommandResult.Error("Usage: chat <scope> <message>");

        var scope = args[0].ToLower();
        var message = string.Join(" ", args.Skip(1));

        if (!new[] { "room", "area", "global" }.Contains(scope))
            return CommandResult.Error("Invalid scope. Use: room, area, or global");

        var output = $"[{scope.ToUpper()}] {character.Name}: {message}";
        return CommandResult.Ok(output);
    }

    public override bool ValidateArgs(string[] args) => true;
}
