namespace VibeMUD.Models;

/// <summary>
/// Represents a player character
/// </summary>
public class Character
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty; // warrior, thief, mage, cleric, monk, druid
    public int Level { get; set; } = 1;
    public long ExperiencePoints { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }

    // Base Stats
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }

    // Equipment Slots
    public Dictionary<string, Item?> Equipment { get; set; } = new()
    {
        ["head"] = null,
        ["chest"] = null,
        ["hands"] = null,
        ["legs"] = null,
        ["feet"] = null,
        ["waist"] = null,
        ["back"] = null,
        ["main-hand"] = null,
        ["off-hand"] = null,
        ["ring1"] = null,
        ["ring2"] = null
    };

    // Inventory
    public List<Item> Inventory { get; set; } = new();
    public double InventoryWeight { get; set; }
    public double MaxInventoryWeight { get; set; } = 100.0;
    public int Gold { get; set; }

    // Skills and Location
    public List<string> SkillIds { get; set; } = new();
    public string? CurrentAreaId { get; set; }
    public string? CurrentRoomId { get; set; }
    public Dictionary<string, DateTime> SkillCooldowns { get; set; } = new();

    public Character() { }

    public Character(string id, string name, string @class)
    {
        Id = id;
        Name = name;
        Class = @class;
    }

    public bool AddToInventory(Item item)
    {
        if (InventoryWeight + item.Weight > MaxInventoryWeight)
            return false;

        Inventory.Add(item);
        InventoryWeight += item.Weight;
        return true;
    }

    public bool RemoveFromInventory(Item item)
    {
        if (!Inventory.Remove(item))
            return false;

        InventoryWeight -= item.Weight;
        return true;
    }

    public bool Equip(Item item, string slot)
    {
        if (!Equipment.ContainsKey(slot))
            return false;

        if (!item.CanEquip(Level))
            return false;

        Equipment[slot] = item;
        return true;
    }

    public bool Unequip(string slot)
    {
        if (!Equipment.ContainsKey(slot))
            return false;

        Equipment[slot] = null;
        return true;
    }

    public int GetTotalArmor()
    {
        int totalArmor = 0;
        foreach (var item in Equipment.Values.Where(e => e != null))
        {
            totalArmor += item!.GetArmor();
        }
        return totalArmor;
    }

    public int GetStatModifier(string statName)
    {
        int modifier = 0;
        foreach (var item in Equipment.Values.Where(e => e != null))
        {
            modifier += item!.GetStatBonus(statName);
        }
        return modifier;
    }

    public int GetEffectiveStat(string statName)
    {
        return statName.ToLower() switch
        {
            "strength" => Strength + GetStatModifier("strength"),
            "dexterity" => Dexterity + GetStatModifier("dexterity"),
            "constitution" => Constitution + GetStatModifier("constitution"),
            "intelligence" => Intelligence + GetStatModifier("intelligence"),
            "wisdom" => Wisdom + GetStatModifier("wisdom"),
            "charisma" => Charisma + GetStatModifier("charisma"),
            _ => 0
        };
    }

    public void Heal(int amount)
    {
        Health = Math.Min(Health + amount, MaxHealth);
    }

    public void RestoreMana(int amount)
    {
        Mana = Math.Min(Mana + amount, MaxMana);
    }

    public bool TakeDamage(int damage)
    {
        Health -= damage;
        return Health <= 0;
    }

    public bool HasSkill(string skillId)
    {
        return SkillIds.Contains(skillId);
    }

    public void LearnSkill(string skillId)
    {
        if (!SkillIds.Contains(skillId))
            SkillIds.Add(skillId);
    }

    public bool IsAlive()
    {
        return Health > 0;
    }
}
