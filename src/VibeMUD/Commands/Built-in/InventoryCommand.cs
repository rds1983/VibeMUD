namespace VibeMUD.Commands;

using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Displays and manages inventory
/// </summary>
public class InventoryCommand : Command
{
    public InventoryCommand()
    {
        Name = "inventory";
        Description = "Display your inventory";
        Aliases = new() { "inv", "i" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (character.Inventory.Count == 0)
            return CommandResult.Ok("Your inventory is empty.");

        var inv = new System.Text.StringBuilder();
        inv.AppendLine($"\n=== INVENTORY (Weight: {character.InventoryWeight:F1}/{character.MaxInventoryWeight:F1}) ===");

        foreach (var item in character.Inventory)
        {
            inv.AppendLine($"  {item.Name} ({item.Type})");
        }

        return CommandResult.Ok(inv.ToString());
    }
}

/// <summary>
/// Equips an item
/// </summary>
public class EquipCommand : Command
{
    public EquipCommand()
    {
        Name = "equip";
        Description = "Equip an item from your inventory";
        Aliases = new() { "wear", "wield" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Error("Usage: equip <item name>");

        var itemName = string.Join(" ", args).ToLower();
        var item = character.Inventory.FirstOrDefault(i => i.Name.ToLower().Contains(itemName));

        if (item == null)
            return CommandResult.Error($"You don't have '{itemName}' in your inventory.");

        if (string.IsNullOrEmpty(item.Slot))
            return CommandResult.Error($"'{item.Name}' cannot be equipped.");

        if (!character.Equip(item, item.Slot))
            return CommandResult.Error($"You cannot equip '{item.Name}' - insufficient level or invalid slot.");

        character.RemoveFromInventory(item);
        return CommandResult.Ok($"You equip the {item.Name}.");
    }
}

/// <summary>
/// Unequips an item
/// </summary>
public class UnequipCommand : Command
{
    public UnequipCommand()
    {
        Name = "unequip";
        Description = "Unequip an item";
        Aliases = new() { "remove", "unwear" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Error("Usage: unequip <slot>");

        var slot = args[0].ToLower();

        if (!character.Equipment.ContainsKey(slot))
            return CommandResult.Error($"Invalid slot: {slot}");

        var item = character.Equipment[slot];
        if (item == null)
            return CommandResult.Error($"Nothing equipped in {slot}.");

        if (!character.Unequip(slot))
            return CommandResult.Error($"Failed to unequip '{item.Name}'.");

        if (!character.AddToInventory(item))
        {
            character.Equipment[slot] = item;
            return CommandResult.Error("Your inventory is full.");
        }

        return CommandResult.Ok($"You unequip the {item.Name}.");
    }
}

/// <summary>
/// Drops an item
/// </summary>
public class DropCommand : Command
{
    public DropCommand()
    {
        Name = "drop";
        Description = "Drop an item from your inventory";
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Error("Usage: drop <item name>");

        var itemName = string.Join(" ", args).ToLower();
        var item = character.Inventory.FirstOrDefault(i => i.Name.ToLower().Contains(itemName));

        if (item == null)
            return CommandResult.Error($"You don't have '{itemName}' in your inventory.");

        if (!character.RemoveFromInventory(item))
            return CommandResult.Error($"Failed to drop '{item.Name}'.");

        if (!string.IsNullOrEmpty(character.CurrentAreaId) && !string.IsNullOrEmpty(character.CurrentRoomId))
        {
            var room = gameState.GetRoom(character.CurrentAreaId, character.CurrentRoomId);
            room?.AddItem(item);
        }

        return CommandResult.Ok($"You drop the {item.Name}.");
    }
}

/// <summary>
/// Picks up an item
/// </summary>
public class TakeCommand : Command
{
    public TakeCommand()
    {
        Name = "take";
        Description = "Pick up an item from the ground";
        Aliases = new() { "get", "pickup" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Error("Usage: take <item name>");

        if (string.IsNullOrEmpty(character.CurrentAreaId) || string.IsNullOrEmpty(character.CurrentRoomId))
            return CommandResult.Error("You are not in a valid location.");

        var room = gameState.GetRoom(character.CurrentAreaId, character.CurrentRoomId);
        if (room == null)
            return CommandResult.Error("The room no longer exists.");

        var itemName = string.Join(" ", args).ToLower();
        var item = room.Items.FirstOrDefault(i => i.Name.ToLower().Contains(itemName));

        if (item == null)
            return CommandResult.Error($"You don't see '{itemName}' here.");

        if (!character.AddToInventory(item))
            return CommandResult.Error("Your inventory is full.");

        room.RemoveItem(item);
        return CommandResult.Ok($"You pick up the {item.Name}.");
    }
}
