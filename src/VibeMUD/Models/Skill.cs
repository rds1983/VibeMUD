namespace VibeMUD.Models;

/// <summary>
/// Represents a character or NPC skill/ability
/// </summary>
public class Skill
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ManaCost { get; set; }
    public int CooldownSeconds { get; set; }
    public int Damage { get; set; }
    public string DamageType { get; set; } = "physical"; // physical, fire, ice, lightning, light
    public bool IsOffensive { get; set; }
    public bool IsHealing { get; set; }
    public int HealAmount { get; set; }
    public int RequiredLevel { get; set; }

    public Skill() { }

    public Skill(string id, string name, string description, int manaCost, int cooldown, int damage, string damageType, bool isOffensive)
    {
        Id = id;
        Name = name;
        Description = description;
        ManaCost = manaCost;
        CooldownSeconds = cooldown;
        Damage = damage;
        DamageType = damageType;
        IsOffensive = isOffensive;
    }

    public bool CanUse(int currentMana, DateTime lastUsed)
    {
        return currentMana >= ManaCost &&
               (DateTime.UtcNow - lastUsed).TotalSeconds >= CooldownSeconds;
    }
}
