namespace VibeMUD.Commands;

using VibeMUD.Core;
using VibeMUD.Models;

/// <summary>
/// Displays character information
/// </summary>
public class InfoCommand : Command
{
    public InfoCommand()
    {
        Name = "info";
        Description = "Display your character information";
        Aliases = new() { "stats", "character", "status" };
    }

    public override CommandResult Execute(Character character, GameState gameState, string[] args)
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"\n=== {character.Name} (Level {character.Level} {character.Class}) ===");
        info.AppendLine($"Experience: {character.ExperiencePoints}");
        info.AppendLine($"Gold: {character.Gold}");
        info.AppendLine();

        // Health and Mana
        info.AppendLine($"Health:  {character.Health}/{character.MaxHealth}");
        info.AppendLine($"Mana:    {character.Mana}/{character.MaxMana}");
        info.AppendLine();

        // Stats
        info.AppendLine("=== STATS ===");
        info.AppendLine($"Strength:     {character.GetEffectiveStat("strength")} (base: {character.Strength})");
        info.AppendLine($"Dexterity:    {character.GetEffectiveStat("dexterity")} (base: {character.Dexterity})");
        info.AppendLine($"Constitution: {character.GetEffectiveStat("constitution")} (base: {character.Constitution})");
        info.AppendLine($"Intelligence: {character.GetEffectiveStat("intelligence")} (base: {character.Intelligence})");
        info.AppendLine($"Wisdom:       {character.GetEffectiveStat("wisdom")} (base: {character.Wisdom})");
        info.AppendLine($"Charisma:     {character.GetEffectiveStat("charisma")} (base: {character.Charisma})");
        info.AppendLine();

        // Combat
        info.AppendLine($"Total Armor: {character.GetTotalArmor()}");
        info.AppendLine();

        // Location
        if (!string.IsNullOrEmpty(character.CurrentAreaId) && !string.IsNullOrEmpty(character.CurrentRoomId))
        {
            if (gameState.Areas.TryGetValue(character.CurrentAreaId, out var area))
            {
                var room = area.GetRoom(character.CurrentRoomId);
                info.AppendLine($"Location: {area.Name} - {room?.Title ?? "Unknown"}");
            }
        }
        else
        {
            info.AppendLine("Location: Unknown");
        }

        return CommandResult.Ok(info.ToString());
    }
}
