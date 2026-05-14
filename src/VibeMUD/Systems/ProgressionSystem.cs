namespace VibeMUD.Systems;

using VibeMUD.Models;

/// <summary>
/// Manages character progression including experience, leveling, and stat growth
/// </summary>
public class ProgressionSystem
{
    // Class-specific stat multipliers for leveling
    private static readonly Dictionary<string, (int str, int dex, int con, int intel, int wis, int cha)>
        ClassStatMultipliers = new()
    {
        ["warrior"] = (3, 1, 2, 1, 1, 1),      // Strength heavy
        ["thief"] = (1, 3, 1, 1, 1, 2),        // Dexterity heavy
        ["mage"] = (1, 1, 1, 3, 2, 1),         // Intelligence heavy
        ["cleric"] = (1, 1, 2, 1, 3, 2),       // Wisdom heavy
        ["monk"] = (2, 2, 2, 1, 2, 1),         // Balanced, slight Dex/Con
        ["druid"] = (1, 1, 2, 2, 2, 2)         // Balanced
    };

    // Base health and mana per level by class
    private static readonly Dictionary<string, (int baseHealth, int baseMana)>
        ClassBaseStat = new()
    {
        ["warrior"] = (15, 5),
        ["thief"] = (10, 10),
        ["mage"] = (8, 20),
        ["cleric"] = (12, 15),
        ["monk"] = (12, 12),
        ["druid"] = (11, 18)
    };

    /// <summary>
    /// Calculates total experience points required to reach a given level
    /// Formula: Each level requires 10% more XP than the previous
    /// </summary>
    public static long GetExperienceForLevel(int level)
    {
        if (level < 1)
            return 0;

        // Level 1 = 0 XP required (starting level)
        if (level == 1)
            return 0;

        // Each level requires 10% more XP than previous
        // Level 2 = 1000, Level 3 = 2100, Level 4 = 3310, etc.
        const long baseXpLevel2 = 1000;
        long totalXp = 0;

        for (int i = 2; i <= level; i++)
        {
            totalXp += (long)(baseXpLevel2 * Math.Pow(1.1, i - 2));
        }

        return totalXp;
    }

    /// <summary>
    /// Awards experience to a character and handles leveling up
    /// </summary>
    public static void AwardExperience(Character character, long experienceGained)
    {
        character.ExperiencePoints += experienceGained;

        // Check if character should level up
        while (character.Level < 100 &&
               character.ExperiencePoints >= GetExperienceForLevel(character.Level + 1))
        {
            LevelUp(character);
        }
    }

    /// <summary>
    /// Levels up a character and increases stats based on class
    /// </summary>
    public static void LevelUp(Character character)
    {
        character.Level++;

        // Get stat multipliers for this class
        var (strMult, dexMult, conMult, intelMult, wisMult, chaMult) =
            ClassStatMultipliers.TryGetValue(character.Class.ToLower(), out var mults)
                ? mults
                : (1, 1, 1, 1, 1, 1);

        // Increase base stats
        character.Strength += strMult;
        character.Dexterity += dexMult;
        character.Constitution += conMult;
        character.Intelligence += intelMult;
        character.Wisdom += wisMult;
        character.Charisma += chaMult;

        // Increase health and mana based on class
        if (ClassBaseStat.TryGetValue(character.Class.ToLower(), out var baseStat))
        {
            character.MaxHealth += baseStat.baseHealth + character.Constitution;
            character.MaxMana += baseStat.baseMana + (character.Intelligence / 2);
        }
        else
        {
            character.MaxHealth += 10 + character.Constitution;
            character.MaxMana += 10 + (character.Intelligence / 2);
        }

        // Restore health and mana on level up
        character.Health = character.MaxHealth;
        character.Mana = character.MaxMana;
    }

    /// <summary>
    /// Calculates experience reward for defeating an NPC based on level difference
    /// </summary>
    public static long CalculateExperienceReward(Character character, NPC npc)
    {
        // Base XP for NPC's level
        long baseXp = (long)(100 * Math.Pow(1.1, npc.Level - 1));

        // Level difference modifier
        int levelDifference = npc.Level - character.Level;
        double modifier = 1.0;

        if (levelDifference > 0)
        {
            // Bonus for fighting higher level enemies: +10% per level above
            modifier = 1.0 + (levelDifference * 0.1);
        }
        else if (levelDifference < -5)
        {
            // Penalty for fighting much lower level enemies
            // No XP if 6+ levels below
            return 0;
        }
        else if (levelDifference < 0)
        {
            // Small penalty for fighting lower level enemies: -5% per level below
            modifier = Math.Max(0.1, 1.0 + (levelDifference * 0.05));
        }

        return (long)(baseXp * modifier);
    }

    /// <summary>
    /// Initializes a new character with starting stats based on class
    /// </summary>
    public static void InitializeCharacter(Character character)
    {
        string classLower = character.Class.ToLower();

        // Set base stats by class
        character.Strength = 10;
        character.Dexterity = 10;
        character.Constitution = 10;
        character.Intelligence = 10;
        character.Wisdom = 10;
        character.Charisma = 10;

        // Apply class-specific starting bonuses
        if (ClassStatMultipliers.TryGetValue(classLower, out var mults))
        {
            character.Strength += mults.str * 2;
            character.Dexterity += mults.dex * 2;
            character.Constitution += mults.con * 2;
            character.Intelligence += mults.intel * 2;
            character.Wisdom += mults.wis * 2;
            character.Charisma += mults.cha * 2;
        }

        // Set health and mana based on class
        if (ClassBaseStat.TryGetValue(classLower, out var baseStat))
        {
            character.MaxHealth = baseStat.baseHealth + character.Constitution;
            character.MaxMana = baseStat.baseMana + (character.Intelligence / 2);
        }
        else
        {
            character.MaxHealth = 20 + character.Constitution;
            character.MaxMana = 10 + (character.Intelligence / 2);
        }

        character.Health = character.MaxHealth;
        character.Mana = character.MaxMana;
        character.Level = 1;
        character.ExperiencePoints = 0;
    }
}
