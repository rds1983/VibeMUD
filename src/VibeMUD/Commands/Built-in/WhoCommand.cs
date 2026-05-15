namespace VibeMUD.Commands.Built_in;

using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// List all online players
/// </summary>
public class WhoCommand : Command
{
    public WhoCommand()
    {
        Name = "who";
        Description = "List all online players";
        Aliases = new();
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        var players = gameState.Characters.Values.ToList();

        if (players.Count == 0)
            return CommandResult.Ok("No players online.");

        var output = new System.Text.StringBuilder();
        output.AppendLine($"=== Online Players ({players.Count}) ===");
        output.AppendLine("");

        foreach (var player in players.OrderByDescending(p => p.Level).ThenBy(p => p.Name))
        {
            output.AppendLine($"  {player.Name,-20} Level {player.Level,-3} {player.Class}");
        }

        return CommandResult.Ok(output.ToString().TrimEnd());
    }

    public override bool ValidateArgs(string[] args) => true;
}
