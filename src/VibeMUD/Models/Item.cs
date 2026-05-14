namespace VibeMUD.Models;

/// <summary>
/// Represents an equipment item or consumable
/// </summary>
public class Item
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // weapon, armor, accessory, consumable
    public string Subtype { get; set; } = string.Empty; // sword, chest, ring, etc.
    public string? Slot { get; set; } // equipment slot: main-hand, off-hand, chest, head, etc.
    public double Weight { get; set; }
    public int Value { get; set; }
    public int RequiredLevel { get; set; }
    public Dictionary<string, int> Stats { get; set; } = new(); // damage, armor, bonus_strength, etc.
    public Dictionary<string, double> Resistances { get; set; } = new(); // fire, ice, lightning, light
    public string? DamageType { get; set; } // physical, fire, ice, lightning, light

    public Item() { }

    public Item(string id, string name, string type, string subtype, int value, int level = 1)
    {
        Id = id;
        Name = name;
        Type = type;
        Subtype = subtype;
        Value = value;
        RequiredLevel = level;
    }

    public bool CanEquip(int playerLevel)
    {
        return playerLevel >= RequiredLevel;
    }

    public int GetDamage()
    {
        return Stats.ContainsKey("damage") ? Stats["damage"] : 0;
    }

    public int GetArmor()
    {
        return Stats.ContainsKey("armor") ? Stats["armor"] : 0;
    }

    public int GetStatBonus(string statName)
    {
        var key = $"bonus_{statName.ToLower()}";
        return Stats.ContainsKey(key) ? Stats[key] : 0;
    }
}
