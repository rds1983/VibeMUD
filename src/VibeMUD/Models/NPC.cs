namespace VibeMUD.Models;

/// <summary>
/// Represents a non-player character (monster/NPC)
/// </summary>
public class NPC
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }

    // Stats
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }

    // Behavior
    public string Behavior { get; set; } = "wander"; // wander, patrol, aggressive
    public List<string> PatrolPath { get; set; } = new();

    // Combat
    public List<string> SkillIds { get; set; } = new();
    public List<string> SpecialAttackIds { get; set; } = new();
    public List<string> Flags { get; set; } = new(); // undead, ethereal, undying, giant, beast, etc.

    // Loot
    public int GoldMin { get; set; }
    public int GoldMax { get; set; }
    public Dictionary<string, double> LootTable { get; set; } = new(); // itemId -> probability

    // Resistances
    public Dictionary<string, double> Resistances { get; set; } = new()
    {
        ["physical"] = 0.0,
        ["fire"] = 0.0,
        ["ice"] = 0.0,
        ["lightning"] = 0.0,
        ["light"] = 0.0
    };

    public List<string> DamageTypes { get; set; } = new() { "physical" };
    public int RespawnTimeSeconds { get; set; } = 300;

    // Runtime state
    public string? CurrentAreaId { get; set; }
    public string? CurrentRoomId { get; set; }
    public DateTime? LastRespawnTime { get; set; }
    public Dictionary<string, DateTime> SkillCooldowns { get; set; } = new();

    public NPC() { }

    public NPC(string id, string name, string description, int level)
    {
        Id = id;
        Name = name;
        Description = description;
        Level = level;
    }

    public bool TakeDamage(int damage, string damageType = "physical")
    {
        double damageMultiplier = 1.0;

        // Apply resistances
        if (Resistances.ContainsKey(damageType))
            damageMultiplier = 1.0 + Resistances[damageType];

        // Special vulnerabilities
        if (damageType == "light" && HasFlag("undead"))
            damageMultiplier *= 1.5;

        int finalDamage = Math.Max(1, (int)(damage * damageMultiplier));
        Health -= finalDamage;
        return Health <= 0;
    }

    public bool HasFlag(string flag)
    {
        return Flags.Contains(flag);
    }

    public bool IsAlive()
    {
        return Health > 0;
    }

    public int GenerateLoot()
    {
        Random random = new();
        return random.Next(GoldMin, GoldMax + 1);
    }

    public List<string> RollLootItems()
    {
        Random random = new();
        List<string> drops = new();

        foreach (var (itemId, probability) in LootTable)
        {
            if (random.NextDouble() <= probability)
                drops.Add(itemId);
        }

        return drops;
    }

    public void RestoreCombat()
    {
        Health = MaxHealth;
        Mana = MaxMana;
        SkillCooldowns.Clear();
    }

    public bool CanUseSkill(string skillId, int manaCost, DateTime lastUsed, int cooldownSeconds)
    {
        return Mana >= manaCost &&
               (DateTime.UtcNow - lastUsed).TotalSeconds >= cooldownSeconds;
    }
}
