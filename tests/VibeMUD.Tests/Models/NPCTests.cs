namespace VibeMUD.Tests.Models;

using VibeMUD.Models;

public class NPCTests
{
    [Fact]
    public void Constructor_WithParameters_InitializesCorrectly()
    {
        var npc = new NPC("goblin_warrior", "Goblin Warrior", "A scrappy green warrior", 3);

        Assert.Equal("goblin_warrior", npc.Id);
        Assert.Equal("Goblin Warrior", npc.Name);
        Assert.Equal("A scrappy green warrior", npc.Description);
        Assert.Equal(3, npc.Level);
    }

    [Fact]
    public void TakeDamage_Normal_ReducesHealth()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 1) { Health = 20, MaxHealth = 20 };

        npc.TakeDamage(5, "physical");

        Assert.Equal(15, npc.Health);
    }

    [Fact]
    public void TakeDamage_WithResistance_ReducesDamage()
    {
        var npc = new NPC("ice_elemental", "Ice Elemental", "desc", 5)
        {
            Health = 50,
            MaxHealth = 50,
            Resistances = new() { ["physical"] = -0.5 }
        };

        npc.TakeDamage(10, "physical");

        Assert.Equal(45, npc.Health); // 10 * (1 - 0.5) = 5 damage
    }

    [Fact]
    public void TakeDamage_UndeadWithLight_TakesExtraDamage()
    {
        var npc = new NPC("skeleton", "Skeleton", "undead", 5)
        {
            Health = 50,
            MaxHealth = 50,
            Flags = new() { "undead" }
        };

        npc.TakeDamage(10, "light");

        Assert.Equal(35, npc.Health); // 10 * 1.5 = 15 damage
    }

    [Fact]
    public void TakeDamage_KillsNPC_ReturnsTrue()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 1) { Health = 10, MaxHealth = 10 };

        bool isDead = npc.TakeDamage(10, "physical");

        Assert.True(isDead);
    }

    [Fact]
    public void HasFlag_ExistingFlag_ReturnsTrue()
    {
        var npc = new NPC("lich", "Lich", "desc", 50) { Flags = new() { "undead", "ethereal" } };

        Assert.True(npc.HasFlag("undead"));
        Assert.True(npc.HasFlag("ethereal"));
    }

    [Fact]
    public void HasFlag_NonExistingFlag_ReturnsFalse()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 1) { Flags = new() { "beast" } };

        Assert.False(npc.HasFlag("undead"));
    }

    [Fact]
    public void IsAlive_WithHealth_ReturnsTrue()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 1) { Health = 10 };

        Assert.True(npc.IsAlive());
    }

    [Fact]
    public void IsAlive_ZeroHealth_ReturnsFalse()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 1) { Health = 0 };

        Assert.False(npc.IsAlive());
    }

    [Fact]
    public void GenerateLoot_ReturnsGoldInRange()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 1) { GoldMin = 5, GoldMax = 15 };

        int gold = npc.GenerateLoot();

        Assert.InRange(gold, 5, 15);
    }

    [Fact]
    public void RollLootItems_RespectsDropProbability()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 1)
        {
            LootTable = new()
            {
                ["guaranteed_item"] = 1.0,
                ["rare_item"] = 0.0
            }
        };

        var drops = npc.RollLootItems();

        Assert.Contains("guaranteed_item", drops);
        Assert.DoesNotContain("rare_item", drops);
    }

    [Fact]
    public void RestoreCombat_ResetsHealthAndMana()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 1)
        {
            Health = 5,
            MaxHealth = 20,
            Mana = 0,
            MaxMana = 10
        };
        npc.SkillCooldowns["fireball"] = DateTime.UtcNow;

        npc.RestoreCombat();

        Assert.Equal(20, npc.Health);
        Assert.Equal(10, npc.Mana);
        Assert.Empty(npc.SkillCooldowns);
    }

    [Fact]
    public void CanUseSkill_WithSufficientResources_ReturnsTrue()
    {
        var npc = new NPC("mage", "Mage", "desc", 5) { Mana = 50 };
        var lastUsed = DateTime.UtcNow.AddSeconds(-10);

        Assert.True(npc.CanUseSkill("fireball", 30, lastUsed, 5));
    }

    [Fact]
    public void CanUseSkill_WithInsufficientMana_ReturnsFalse()
    {
        var npc = new NPC("mage", "Mage", "desc", 5) { Mana = 10 };
        var lastUsed = DateTime.UtcNow.AddSeconds(-10);

        Assert.False(npc.CanUseSkill("fireball", 30, lastUsed, 5));
    }
}
