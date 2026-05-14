namespace VibeMUD.Tests.Systems;

using VibeMUD.Models;
using VibeMUD.Systems;

public class ProgressionSystemTests
{
    [Fact]
    public void GetExperienceForLevel_Level1_ReturnsZero()
    {
        long xp = ProgressionSystem.GetExperienceForLevel(1);
        Assert.Equal(0, xp);
    }

    [Fact]
    public void GetExperienceForLevel_Level2_Returns1000()
    {
        long xp = ProgressionSystem.GetExperienceForLevel(2);
        Assert.Equal(1000, xp);
    }

    [Fact]
    public void GetExperienceForLevel_IncreasesByTenPercentPerLevel()
    {
        long xp3 = ProgressionSystem.GetExperienceForLevel(3);
        long xp2 = ProgressionSystem.GetExperienceForLevel(2);

        // Level 3 should be 1000 + 1100 = 2100
        Assert.Equal(2100, xp3);

        // Verify progression
        long xp4 = ProgressionSystem.GetExperienceForLevel(4);
        Assert.Equal(3310, xp4); // 1000 + 1100 + 1210
    }

    [Fact]
    public void AwardExperience_BelowLevelUpThreshold_DoesNotLevelUp()
    {
        var character = new Character("1", "TestChar", "warrior");
        character.Level = 1;
        character.ExperiencePoints = 0;

        ProgressionSystem.AwardExperience(character, 500);

        Assert.Equal(1, character.Level);
        Assert.Equal(500, character.ExperiencePoints);
    }

    [Fact]
    public void AwardExperience_ExactlyEnough_LevelsUp()
    {
        var character = new Character("1", "TestChar", "warrior");
        character.Level = 1;
        character.ExperiencePoints = 0;

        ProgressionSystem.AwardExperience(character, 1000);

        Assert.Equal(2, character.Level);
    }

    [Fact]
    public void AwardExperience_ExceedingThreshold_LevelsUpMultipleTimes()
    {
        var character = new Character("1", "TestChar", "warrior");
        character.Level = 1;
        character.ExperiencePoints = 0;

        // Award 3310 XP (enough for levels 2, 3, and 4)
        ProgressionSystem.AwardExperience(character, 3310);

        Assert.Equal(4, character.Level);
    }

    [Fact]
    public void LevelUp_Warrior_IncreasesStrengthMost()
    {
        var character = new Character("1", "TestChar", "warrior");
        ProgressionSystem.InitializeCharacter(character);

        int initialStr = character.Strength;
        int initialDex = character.Dexterity;

        ProgressionSystem.LevelUp(character);

        // Warrior gets +3 STR per level
        Assert.Equal(initialStr + 3, character.Strength);
        // Warrior gets +1 DEX per level
        Assert.Equal(initialDex + 1, character.Dexterity);
    }

    [Fact]
    public void LevelUp_Mage_IncreasesIntelligenceMost()
    {
        var character = new Character("1", "TestChar", "mage");
        ProgressionSystem.InitializeCharacter(character);

        int initialIntel = character.Intelligence;
        int initialStr = character.Strength;

        ProgressionSystem.LevelUp(character);

        // Mage gets +3 INT per level
        Assert.Equal(initialIntel + 3, character.Intelligence);
        // Mage gets +1 STR per level
        Assert.Equal(initialStr + 1, character.Strength);
    }

    [Fact]
    public void LevelUp_IncreasesHealthAndMana()
    {
        var character = new Character("1", "TestChar", "warrior");
        ProgressionSystem.InitializeCharacter(character);

        int initialMaxHealth = character.MaxHealth;
        int initialMaxMana = character.MaxMana;

        ProgressionSystem.LevelUp(character);

        Assert.True(character.MaxHealth > initialMaxHealth);
        Assert.True(character.MaxMana >= initialMaxMana); // Mana might not change if INT bonus is small
    }

    [Fact]
    public void LevelUp_RestoreshHealthAndManaToMax()
    {
        var character = new Character("1", "TestChar", "warrior");
        ProgressionSystem.InitializeCharacter(character);

        character.Health = 1;
        character.Mana = 1;

        ProgressionSystem.LevelUp(character);

        Assert.Equal(character.MaxHealth, character.Health);
        Assert.Equal(character.MaxMana, character.Mana);
    }

    [Fact]
    public void CalculateExperienceReward_SameLevelEnemy_ReturnsBaseXP()
    {
        var character = new Character("1", "TestChar", "warrior") { Level = 5 };
        var npc = new NPC("npc1", "Goblin", "A small goblin", 5);

        long xp = ProgressionSystem.CalculateExperienceReward(character, npc);

        // Base XP for level 5: 100 * 1.1^4 = ~146
        Assert.InRange(xp, 140, 160);
    }

    [Fact]
    public void CalculateExperienceReward_HigherLevelEnemy_BonusMultiplier()
    {
        var character = new Character("1", "TestChar", "warrior") { Level = 5 };
        var npcSameLevel = new NPC("npc1", "Goblin", "A small goblin", 5);
        var npcHigherLevel = new NPC("npc2", "Orc", "A fierce orc", 7);

        long xpSame = ProgressionSystem.CalculateExperienceReward(character, npcSameLevel);
        long xpHigher = ProgressionSystem.CalculateExperienceReward(character, npcHigherLevel);

        // Higher level enemies give more XP
        Assert.True(xpHigher > xpSame);
    }

    [Fact]
    public void CalculateExperienceReward_LowerLevelEnemy_PenaltyMultiplier()
    {
        var character = new Character("1", "TestChar", "warrior") { Level = 5 };
        var npcSameLevel = new NPC("npc1", "Goblin", "A small goblin", 5);
        var npcLowerLevel = new NPC("npc2", "Rat", "A small rat", 3);

        long xpSame = ProgressionSystem.CalculateExperienceReward(character, npcSameLevel);
        long xpLower = ProgressionSystem.CalculateExperienceReward(character, npcLowerLevel);

        // Lower level enemies give less XP
        Assert.True(xpLower < xpSame);
    }

    [Fact]
    public void CalculateExperienceReward_MuchLowerLevel_NoXP()
    {
        var character = new Character("1", "TestChar", "warrior") { Level = 10 };
        var npc = new NPC("npc1", "Rat", "A small rat", 3);

        long xp = ProgressionSystem.CalculateExperienceReward(character, npc);

        // More than 6 levels below gives no XP
        Assert.Equal(0, xp);
    }

    [Fact]
    public void InitializeCharacter_Warrior_HasWarriorStats()
    {
        var character = new Character("1", "TestChar", "warrior");
        ProgressionSystem.InitializeCharacter(character);

        // Warrior should have high STR and CON
        Assert.True(character.Strength > character.Intelligence);
        Assert.True(character.Constitution > character.Dexterity);
        Assert.Equal(1, character.Level);
        Assert.Equal(0, character.ExperiencePoints);
    }

    [Fact]
    public void InitializeCharacter_Mage_HasMageStats()
    {
        var character = new Character("1", "TestChar", "mage");
        ProgressionSystem.InitializeCharacter(character);

        // Mage should have high INT and WIS, high MaxMana
        Assert.True(character.Intelligence > character.Strength);
        Assert.True(character.MaxMana > 15);
        Assert.Equal(1, character.Level);
    }

    [Fact]
    public void InitializeCharacter_SetsHealthAndMana()
    {
        var character = new Character("1", "TestChar", "warrior");
        ProgressionSystem.InitializeCharacter(character);

        Assert.True(character.MaxHealth > 0);
        Assert.True(character.MaxMana > 0);
        Assert.Equal(character.MaxHealth, character.Health);
        Assert.Equal(character.MaxMana, character.Mana);
    }

    [Fact]
    public void AwardExperience_AccumulatesCorrectly()
    {
        var character = new Character("1", "TestChar", "warrior");
        ProgressionSystem.InitializeCharacter(character);

        ProgressionSystem.AwardExperience(character, 500);
        long xpAfterFirst = character.ExperiencePoints;

        ProgressionSystem.AwardExperience(character, 600);

        Assert.Equal(xpAfterFirst + 600, character.ExperiencePoints);
    }

    [Fact]
    public void MultipleClasses_HaveDifferentProgression()
    {
        var warrior = new Character("w", "WarriorChar", "warrior");
        var mage = new Character("m", "MageChar", "mage");

        ProgressionSystem.InitializeCharacter(warrior);
        ProgressionSystem.InitializeCharacter(mage);

        Assert.NotEqual(warrior.Strength, mage.Strength);
        Assert.NotEqual(warrior.Intelligence, mage.Intelligence);
        Assert.NotEqual(warrior.MaxHealth, mage.MaxHealth);
        Assert.NotEqual(warrior.MaxMana, mage.MaxMana);
    }

    [Fact]
    public void LevelUp_CapAtLevel100()
    {
        var character = new Character("1", "TestChar", "warrior");
        ProgressionSystem.InitializeCharacter(character);
        character.Level = 100;
        character.ExperiencePoints = 0;

        long xpForLevel101 = ProgressionSystem.GetExperienceForLevel(101);
        ProgressionSystem.AwardExperience(character, xpForLevel101 + 1000);

        Assert.Equal(100, character.Level); // Should cap at 100
    }
}
