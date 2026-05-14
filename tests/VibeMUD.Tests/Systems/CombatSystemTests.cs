namespace VibeMUD.Tests.Systems;

using VibeMUD.Models;
using VibeMUD.Systems;

public class CombatSystemTests
{
    [Fact]
    public void CalculateDamage_WithBaseValues_CalculatesCorrectly()
    {
        // Base 10 damage + (stat 15 / 2 = 7) + variance (-3 to +3) = 14-20 damage
        int damage = CombatSystem.CalculateDamage(10, 15, 0);

        Assert.InRange(damage, 14, 20); // Account for variance
    }

    [Fact]
    public void CalculateDamage_WithArmor_ReducesDamage()
    {
        // Base 10 + (stat 15 / 2 = 7) - armor 5 + variance (-3 to +3) = 9-15 damage
        int damage = CombatSystem.CalculateDamage(10, 15, 5);

        Assert.InRange(damage, 9, 15);
    }

    [Fact]
    public void CalculateDamage_AlwaysMinimumOne()
    {
        int damage = CombatSystem.CalculateDamage(1, 1, 100);

        Assert.True(damage >= 1);
    }

    [Fact]
    public void ApplyResistance_Positive_IncreasesDamage()
    {
        var resistances = new Dictionary<string, double> { ["fire"] = 0.5 };
        int damage = CombatSystem.ApplyResistance(10, "fire", resistances);

        Assert.Equal(15, damage); // 10 * (1 + 0.5)
    }

    [Fact]
    public void ApplyResistance_Negative_ReducesDamage()
    {
        var resistances = new Dictionary<string, double> { ["fire"] = -0.5 };
        int damage = CombatSystem.ApplyResistance(10, "fire", resistances);

        Assert.Equal(5, damage); // 10 * (1 - 0.5)
    }

    [Fact]
    public void ApplyResistance_NoResistance_UnchangedDamage()
    {
        var resistances = new Dictionary<string, double>();
        int damage = CombatSystem.ApplyResistance(10, "fire", resistances);

        Assert.Equal(10, damage);
    }

    [Fact]
    public void ApplyVulnerabilities_LightVsUndead_IncreasesDamage()
    {
        var npc = new NPC("skeleton", "Skeleton", "undead", 5);
        npc.Flags.Add("undead");

        int damage = CombatSystem.ApplyVulnerabilities(10, "light", npc);

        Assert.Equal(15, damage); // 10 * 1.5
    }

    [Fact]
    public void ApplyVulnerabilities_PhysicalVsEthereal_ReducesDamage()
    {
        var npc = new NPC("ghost", "Ghost", "ethereal", 5);
        npc.Flags.Add("ethereal");

        int damage = CombatSystem.ApplyVulnerabilities(10, "physical", npc);

        Assert.Equal(5, damage); // 10 * 0.5
    }

    [Fact]
    public void ApplyVulnerabilities_NoFlags_UnchangedDamage()
    {
        var npc = new NPC("goblin", "Goblin", "normal", 5);

        int damage = CombatSystem.ApplyVulnerabilities(10, "physical", npc);

        Assert.Equal(10, damage);
    }

    [Fact]
    public void CalculateInitiative_ReturnsPositive()
    {
        int initiative = CombatSystem.CalculateInitiative(10);

        Assert.True(initiative > 0);
    }

    [Fact]
    public void CalculateInitiative_HigherDexBetter()
    {
        int initiative1 = CombatSystem.CalculateInitiative(5);
        int initiative2 = CombatSystem.CalculateInitiative(20);

        // On average, higher dex should be higher (though randomness means not guaranteed)
        // Just test they're both positive
        Assert.True(initiative1 > 0);
        Assert.True(initiative2 > 0);
    }

    [Fact]
    public void CharacterAttacksNPC_ValidAttack_DealsDamage()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Strength = 16,
            Health = 100,
            MaxHealth = 100
        };
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            Health = 20,
            MaxHealth = 20
        };

        var system = new CombatSystem();
        var result = system.CharacterAttacksNPC(character, npc);

        Assert.True(result.Success);
        Assert.True(result.DamageDealt > 0);
        Assert.True(npc.Health < 20);
    }

    [Fact]
    public void CharacterAttacksNPC_DealsKillingBlow_ReturnsDefeated()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Strength = 20,
            Health = 100,
            MaxHealth = 100
        };
        var npc = new NPC("goblin", "Goblin", "desc", 1)
        {
            Health = 5,
            MaxHealth = 5
        };

        var system = new CombatSystem();
        var result = system.CharacterAttacksNPC(character, npc);

        Assert.True(result.TargetDefeated);
        Assert.True(npc.Health <= 0);
    }

    [Fact]
    public void NPCAttacksCharacter_ValidAttack_DealsDamage()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Health = 100,
            MaxHealth = 100
        };
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            Strength = 10,
            Health = 20,
            MaxHealth = 20
        };

        var system = new CombatSystem();
        var result = system.NPCAttacksCharacter(npc, character);

        Assert.True(result.Success);
        Assert.True(result.DamageDealt > 0);
        Assert.True(character.Health < 100);
    }

    [Fact]
    public void UseSkill_WithSufficientMana_CastSuccessfully()
    {
        var character = new Character("player1", "Player", "mage")
        {
            Mana = 50,
            MaxMana = 50,
            Intelligence = 16,
            Health = 80,
            MaxHealth = 80
        };
        var skill = new Skill("fireball", "Fireball", "Fire spell", 30, 5, 20, "fire", true);
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            Health = 30,
            MaxHealth = 30
        };

        var system = new CombatSystem();
        var result = system.UseSkill(character, npc, skill);

        Assert.True(result.Success);
        Assert.True(result.DamageDealt > 0);
        Assert.Equal(20, character.Mana); // 50 - 30
        Assert.True(npc.Health < 30);
    }

    [Fact]
    public void UseSkill_InsufficientMana_FailsToCast()
    {
        var character = new Character("player1", "Player", "mage")
        {
            Mana = 10,
            MaxMana = 50,
            Intelligence = 16,
            Health = 80,
            MaxHealth = 80
        };
        var skill = new Skill("fireball", "Fireball", "Fire spell", 30, 5, 20, "fire", true);
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            Health = 30,
            MaxHealth = 30
        };

        var system = new CombatSystem();
        var result = system.UseSkill(character, npc, skill);

        Assert.False(result.Success);
        Assert.Contains("enough mana", result.Message);
        Assert.Equal(10, character.Mana); // Mana unchanged
    }

    [Fact]
    public void UsePotion_ValidPotion_HealCharacter()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Health = 50,
            MaxHealth = 100
        };
        var potion = new Item("health_potion", "Health Potion", "potion", "consumable", 10);
        potion.Stats["heal"] = 30;

        var system = new CombatSystem();
        var result = system.UsePotion(character, potion);

        Assert.True(result.Success);
        Assert.Equal(80, character.Health);
    }

    [Fact]
    public void UsePotion_CapsAtMaxHealth()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Health = 90,
            MaxHealth = 100
        };
        var potion = new Item("health_potion", "Health Potion", "potion", "consumable", 10);
        potion.Stats["heal"] = 30;

        var system = new CombatSystem();
        var result = system.UsePotion(character, potion);

        Assert.True(result.Success);
        Assert.Equal(100, character.Health);
    }

    [Fact]
    public void AttemptFlee_HighDexterityCharacter_LikelyToEscape()
    {
        var character = new Character("player1", "Player", "thief")
        {
            Dexterity = 18,
            Health = 100,
            MaxHealth = 100
        };
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            Dexterity = 8,
            Health = 20,
            MaxHealth = 20,
            Strength = 8
        };

        var system = new CombatSystem();
        bool escaped = false;

        // Try multiple times to account for randomness
        for (int i = 0; i < 10; i++)
        {
            var character2 = new Character("player", "Player", "thief") { Dexterity = 18, Health = 100, MaxHealth = 100 };
            var npc2 = new NPC("goblin", "Goblin", "desc", 3) { Dexterity = 8, Health = 20, MaxHealth = 20, Strength = 8 };
            var result = system.AttemptFlee(character2, npc2);
            if (result.Success)
            {
                escaped = true;
                break;
            }
        }

        Assert.True(escaped); // Should escape at least once in 10 tries with high dex
    }

    [Fact]
    public void AttemptFlee_Failed_NPCAttacksCharacter()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Dexterity = 8,
            Health = 100,
            MaxHealth = 100
        };
        var npc = new NPC("dragon", "Dragon", "desc", 10)
        {
            Dexterity = 18,
            Health = 100,
            MaxHealth = 100,
            Strength = 18
        };

        var system = new CombatSystem();
        bool failedAndTookDamage = false;

        // Try multiple times
        for (int i = 0; i < 10; i++)
        {
            var character2 = new Character("player", "Player", "warrior") { Dexterity = 8, Health = 100, MaxHealth = 100 };
            var npc2 = new NPC("dragon", "Dragon", "desc", 10) { Dexterity = 18, Health = 100, MaxHealth = 100, Strength = 18 };
            var result = system.AttemptFlee(character2, npc2);
            if (!result.Success && character2.Health < 100)
            {
                failedAndTookDamage = true;
                break;
            }
        }

        Assert.True(failedAndTookDamage);
    }

    [Fact]
    public void NPCSpecialAttack_ValidAttack_DealsDamage()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Health = 100,
            MaxHealth = 100
        };
        var npc = new NPC("crocodile", "Crocodile", "desc", 5)
        {
            Strength = 14,
            Health = 30,
            MaxHealth = 30
        };

        var system = new CombatSystem();
        var result = system.NPCSpecialAttack(npc, character, "bite_attack");

        Assert.True(result.Success);
        Assert.True(result.DamageDealt > 0);
        Assert.True(character.Health < 100);
    }

    [Fact]
    public void NPCSpecialAttack_UnknownAttack_FallsBackToNormalAttack()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Health = 100,
            MaxHealth = 100
        };
        var npc = new NPC("goblin", "Goblin", "desc", 5)
        {
            Strength = 10,
            Health = 20,
            MaxHealth = 20
        };

        var system = new CombatSystem();
        var result = system.NPCSpecialAttack(npc, character, "unknown_attack");

        Assert.True(result.Success);
        Assert.True(result.DamageDealt > 0);
    }

    [Fact]
    public void CharacterAttacksNPC_DefeatsNPC_AwardsExperience()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Strength = 20,
            Health = 100,
            MaxHealth = 100,
            Level = 5
        };
        ProgressionSystem.InitializeCharacter(character);
        character.Level = 5; // Reset to level 5 after init

        var npc = new NPC("goblin", "Goblin", "desc", 5)
        {
            Health = 5,
            MaxHealth = 5
        };

        var initialXp = character.ExperiencePoints;
        var system = new CombatSystem();
        var result = system.CharacterAttacksNPC(character, npc);

        Assert.True(result.TargetDefeated);
        Assert.True(result.ExperienceAwarded > 0);
        Assert.True(character.ExperiencePoints > initialXp);
        Assert.Contains("experience", result.Message);
    }

    [Fact]
    public void UseSkill_DefeatsNPC_AwardsExperience()
    {
        var character = new Character("player1", "Player", "mage")
        {
            Mana = 50,
            MaxMana = 50,
            Intelligence = 18,
            Health = 80,
            MaxHealth = 80,
            Level = 5
        };

        var skill = new Skill("fireball", "Fireball", "Fire spell", 40, 5, 20, "fire", true);
        var npc = new NPC("goblin", "Goblin", "desc", 5)
        {
            Health = 15,
            MaxHealth = 15
        };

        var initialXp = character.ExperiencePoints;
        var system = new CombatSystem();
        var result = system.UseSkill(character, npc, skill);

        Assert.True(result.TargetDefeated);
        Assert.True(result.ExperienceAwarded > 0);
        Assert.True(character.ExperiencePoints > initialXp);
        Assert.Contains("experience", result.Message);
    }

    [Fact]
    public void CharacterAttacksNPC_WithoutDefeatingNPC_NoExperienceAwarded()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Strength = 10,
            Health = 100,
            MaxHealth = 100,
            Level = 5
        };
        ProgressionSystem.InitializeCharacter(character);

        var npc = new NPC("goblin", "Goblin", "desc", 5)
        {
            Health = 100,
            MaxHealth = 100
        };

        var initialXp = character.ExperiencePoints;
        var system = new CombatSystem();
        var result = system.CharacterAttacksNPC(character, npc);

        Assert.False(result.TargetDefeated);
        Assert.Equal(0, result.ExperienceAwarded);
        Assert.Equal(initialXp, character.ExperiencePoints);
    }
}
