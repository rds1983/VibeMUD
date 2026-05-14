namespace VibeMUD.Commands;

using VibeMUD.Core;
using VibeMUD.Models;
using VibeMUD.Systems;

/// <summary>
/// Character attacks an NPC
/// </summary>
public class AttackCommand : Command
{
    public AttackCommand()
    {
        Name = "attack";
        Description = "Attack a creature in the current room";
        Aliases = new() { "hit", "strike", "fight" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Error("Usage: attack <target>");

        if (string.IsNullOrEmpty(character.CurrentAreaId) || string.IsNullOrEmpty(character.CurrentRoomId))
            return CommandResult.Error("You are not in a valid location.");

        var room = gameState.GetRoom(character.CurrentAreaId, character.CurrentRoomId);
        if (room == null)
            return CommandResult.Error("The room no longer exists.");

        var targetName = string.Join(" ", args).ToLower();
        var npcs = gameState.GetNPCsInRoom(character.CurrentAreaId, character.CurrentRoomId);
        var target = npcs.FirstOrDefault(n => n.Name.ToLower().Contains(targetName));

        if (target == null)
            return CommandResult.Error($"You don't see '{targetName}' here.");

        var combatSystem = new CombatSystem();
        var result = combatSystem.CharacterAttacksNPC(character, target);

        if (result.TargetDefeated)
        {
            // Grant XP and loot
            int xpGain = (target.Level * 10) + 50;
            character.ExperiencePoints += xpGain;

            var message = new System.Text.StringBuilder();
            message.AppendLine(result.Message);
            message.AppendLine($"You gain {xpGain} experience points!");

            // Generate loot
            int gold = target.GenerateLoot();
            character.Gold += gold;
            message.AppendLine($"You receive {gold} gold!");

            // Remove NPC from room
            room.RemoveNPC(target.Id);

            return CommandResult.Ok(message.ToString());
        }

        return CommandResult.Ok(result.Message);
    }
}

/// <summary>
/// Character casts a spell/skill
/// </summary>
public class CastCommand : Command
{
    public CastCommand()
    {
        Name = "cast";
        Description = "Cast a spell/skill";
        Aliases = new() { "spell" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Error("Usage: cast <skill> [target]");

        if (string.IsNullOrEmpty(character.CurrentAreaId) || string.IsNullOrEmpty(character.CurrentRoomId))
            return CommandResult.Error("You are not in a valid location.");

        var skillName = args[0].ToLower();
        var skill = gameState.Skills.Values.FirstOrDefault(s => s.Name.ToLower() == skillName);

        if (skill == null)
            return CommandResult.Error($"You don't know the '{skillName}' skill.");

        if (!character.HasSkill(skill.Id))
            return CommandResult.Error($"You haven't learned '{skill.Name}' yet.");

        var room = gameState.GetRoom(character.CurrentAreaId, character.CurrentRoomId);
        if (room == null)
            return CommandResult.Error("The room no longer exists.");

        var npcs = gameState.GetNPCsInRoom(character.CurrentAreaId, character.CurrentRoomId);
        if (npcs.Count == 0)
            return CommandResult.Error("There are no targets here.");

        var target = npcs.First(); // Target first NPC for now
        if (args.Length > 1)
        {
            var targetName = string.Join(" ", args.Skip(1)).ToLower();
            target = npcs.FirstOrDefault(n => n.Name.ToLower().Contains(targetName)) ?? target;
        }

        var combatSystem = new CombatSystem();
        var result = combatSystem.UseSkill(character, target, skill);

        if (!result.Success)
            return CommandResult.Error(result.Message);

        if (result.TargetDefeated)
        {
            int xpGain = (target.Level * 10) + 50;
            character.ExperiencePoints += xpGain;

            var message = new System.Text.StringBuilder();
            message.AppendLine(result.Message);
            message.AppendLine($"You gain {xpGain} experience points!");

            int gold = target.GenerateLoot();
            character.Gold += gold;
            message.AppendLine($"You receive {gold} gold!");

            room.RemoveNPC(target.Id);

            return CommandResult.Ok(message.ToString());
        }

        return CommandResult.Ok(result.Message);
    }
}

/// <summary>
/// Character uses an item (potion, scroll, etc.)
/// </summary>
public class UseCommand : Command
{
    public UseCommand()
    {
        Name = "use";
        Description = "Use an item from your inventory";
        Aliases = new() { "drink", "consume", "activate" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (args.Length == 0)
            return CommandResult.Error("Usage: use <item>");

        var itemName = string.Join(" ", args).ToLower();
        var item = character.Inventory.FirstOrDefault(i => i.Name.ToLower().Contains(itemName));

        if (item == null)
            return CommandResult.Error($"You don't have '{itemName}' in your inventory.");

        // Determine item type and use accordingly
        if (item.Type == "potion")
        {
            if (!item.Stats.TryGetValue("heal", out var healAmount))
                return CommandResult.Error($"'{item.Name}' cannot be used.");

            character.Heal(healAmount);
            character.RemoveFromInventory(item);
            return CommandResult.Ok($"You use the {item.Name} and recover {healAmount} health!");
        }

        if (item.Type == "scroll")
        {
            var skill = gameState.Skills.Values.FirstOrDefault(s => item.Name.ToLower().Contains(s.Name.ToLower()));
            if (skill == null)
                return CommandResult.Error($"'{item.Name}' has no effect.");

            if (character.Mana < skill.ManaCost)
                return CommandResult.Error($"You need {skill.ManaCost} mana to use this scroll!");

            var room = gameState.GetRoom(character.CurrentAreaId!, character.CurrentRoomId!);
            var npcs = gameState.GetNPCsInRoom(character.CurrentAreaId!, character.CurrentRoomId!);

            if (npcs.Count == 0)
                return CommandResult.Error("There are no targets here.");

            var target = npcs.First();
            var combatSystem = new CombatSystem();
            var result = combatSystem.UseSkill(character, target, skill);

            character.RemoveFromInventory(item);

            if (result.TargetDefeated)
            {
                room?.RemoveNPC(target.Id);
                return CommandResult.Ok($"{result.Message}\nThe scroll crumbles to dust.");
            }

            return CommandResult.Ok($"{result.Message}\nThe scroll crumbles to dust.");
        }

        return CommandResult.Error($"'{item.Name}' cannot be used.");
    }
}

/// <summary>
/// Character flees from combat
/// </summary>
public class FleeCommand : Command
{
    public FleeCommand()
    {
        Name = "flee";
        Description = "Attempt to flee from combat";
        Aliases = new() { "run", "escape" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        if (string.IsNullOrEmpty(character.CurrentAreaId) || string.IsNullOrEmpty(character.CurrentRoomId))
            return CommandResult.Error("You are not in a valid location.");

        var room = gameState.GetRoom(character.CurrentAreaId, character.CurrentRoomId);
        if (room == null)
            return CommandResult.Error("The room no longer exists.");

        var npcs = gameState.GetNPCsInRoom(character.CurrentAreaId, character.CurrentRoomId);
        if (npcs.Count == 0)
            return CommandResult.Error("There's nothing to flee from!");

        var target = npcs.First();
        var combatSystem = new CombatSystem();
        var result = combatSystem.AttemptFlee(character, target);

        return CommandResult.Ok(result.Message);
    }
}
