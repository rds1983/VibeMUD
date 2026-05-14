namespace VibeMUD.Commands;

using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Displays help information
/// </summary>
public class HelpCommand : Command
{
    public HelpCommand()
    {
        Name = "help";
        Description = "Displays help information about commands";
        Aliases = new() { "h", "?" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        var help = @"
=== VIBE MUD HELP ===

MOVEMENT:
  north, south, east, west, up, down - Move in a direction
  n, s, e, w, u, d - Shorthand directions

INVENTORY:
  inventory, inv, i - Show your inventory
  equip <item> - Equip an item
  unequip <slot> - Unequip an item
  drop <item> - Drop an item

INFORMATION:
  look, l - Examine the current room
  info - Show character information
  help, h, ? - Show this help message

TYPE 'help <command>' for more information about a specific command.
";
        return CommandResult.Ok(help);
    }
}
