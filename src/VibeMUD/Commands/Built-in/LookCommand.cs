namespace VibeMUD.Commands;

using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Examines the current room
/// </summary>
public class LookCommand : Command
{
    public LookCommand()
    {
        Name = "look";
        Description = "Examine the current room";
        Aliases = new() { "l" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (string.IsNullOrEmpty(character.CurrentAreaId) || string.IsNullOrEmpty(character.CurrentRoomId))
            return CommandResult.Error("You are not in a valid location.");

        var room = gameState.GetRoom(character.CurrentAreaId, character.CurrentRoomId);
        if (room == null)
            return CommandResult.Error("The room no longer exists.");

        var description = new System.Text.StringBuilder();
        description.AppendLine($"\n=== {room.Title} ===");
        description.AppendLine(room.Description);

        // Show items
        if (room.Items.Count > 0)
        {
            description.AppendLine("\nItems here:");
            foreach (var item in room.Items)
            {
                description.AppendLine($"  - {item.Name}");
            }
        }

        // Show NPCs
        var npcs = gameState.GetNPCsInRoom(character.CurrentAreaId, character.CurrentRoomId);
        if (npcs.Count > 0)
        {
            description.AppendLine("\nCreatures:");
            foreach (var npc in npcs)
            {
                description.AppendLine($"  - {npc.Name} (Level {npc.Level})");
            }
        }

        // Show other players
        var players = gameState.GetPlayersInRoom(character.CurrentAreaId, character.CurrentRoomId);
        var otherPlayers = players.Where(p => p.Id != character.Id).ToList();
        if (otherPlayers.Count > 0)
        {
            description.AppendLine("\nOther adventurers:");
            foreach (var player in otherPlayers)
            {
                description.AppendLine($"  - {player.Name} (Level {player.Level})");
            }
        }

        // Show exits
        description.AppendLine($"\n{room.GetExitDescription()}");

        return CommandResult.Ok(description.ToString());
    }
}
