namespace VibeMUD.Tests.Models;

using VibeMUD.Models;

public class CharacterTests
{
    [Fact]
    public void Constructor_WithParameters_InitializesCorrectly()
    {
        var character = new Character("player1", "Aragorn", "warrior");

        Assert.Equal("player1", character.Id);
        Assert.Equal("Aragorn", character.Name);
        Assert.Equal("warrior", character.Class);
        Assert.Equal(1, character.Level);
    }

    [Fact]
    public void AddToInventory_WithinWeight_Succeeds()
    {
        var character = new Character("player1", "Aragorn", "warrior");
        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50);
        item.Weight = 3.5;

        Assert.True(character.AddToInventory(item));
        Assert.Contains(item, character.Inventory);
        Assert.Equal(3.5, character.InventoryWeight);
    }

    [Fact]
    public void AddToInventory_ExceedsWeight_Fails()
    {
        var character = new Character("player1", "Aragorn", "warrior");
        character.MaxInventoryWeight = 5;
        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50);
        item.Weight = 10;

        Assert.False(character.AddToInventory(item));
        Assert.DoesNotContain(item, character.Inventory);
    }

    [Fact]
    public void RemoveFromInventory_ExistingItem_Succeeds()
    {
        var character = new Character("player1", "Aragorn", "warrior");
        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50);
        item.Weight = 3.5;
        character.AddToInventory(item);

        Assert.True(character.RemoveFromInventory(item));
        Assert.DoesNotContain(item, character.Inventory);
        Assert.Equal(0, character.InventoryWeight);
    }

    [Fact]
    public void Equip_ValidItem_Succeeds()
    {
        var character = new Character("player1", "Aragorn", "warrior") { Level = 1 };
        var sword = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50, 1);
        sword.Slot = "main-hand";

        Assert.True(character.Equip(sword, "main-hand"));
        Assert.Equal(sword, character.Equipment["main-hand"]);
    }

    [Fact]
    public void Equip_InsufficientLevel_Fails()
    {
        var character = new Character("player1", "Aragorn", "warrior") { Level = 10 };
        var sword = new Item("legendary_sword", "Legendary Sword", "weapon", "sword", 500, 50);

        Assert.False(character.Equip(sword, "main-hand"));
    }

    [Fact]
    public void Unequip_EquippedItem_Succeeds()
    {
        var character = new Character("player1", "Aragorn", "warrior") { Level = 1 };
        var sword = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50, 1);
        character.Equipment["main-hand"] = sword;

        Assert.True(character.Unequip("main-hand"));
        Assert.Null(character.Equipment["main-hand"]);
    }

    [Fact]
    public void GetTotalArmor_MultipleItems_SumsCorrectly()
    {
        var character = new Character("player1", "Aragorn", "warrior");
        var chest = new Item("leather_armor", "Leather Armor", "armor", "chest", 30);
        chest.Stats["armor"] = 5;
        var helmet = new Item("iron_helmet", "Iron Helmet", "armor", "head", 20);
        helmet.Stats["armor"] = 3;

        character.Equipment["chest"] = chest;
        character.Equipment["head"] = helmet;

        Assert.Equal(8, character.GetTotalArmor());
    }

    [Fact]
    public void GetEffectiveStat_WithModifiers_CalculatesCorrectly()
    {
        var character = new Character("player1", "Aragorn", "warrior") { Strength = 18 };
        var sword = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50);
        sword.Stats["bonus_strength"] = 2;
        character.Equipment["main-hand"] = sword;

        Assert.Equal(20, character.GetEffectiveStat("strength"));
    }

    [Fact]
    public void TakeDamage_ReducesHealth()
    {
        var character = new Character("player1", "Aragorn", "warrior") { Health = 100, MaxHealth = 100 };

        character.TakeDamage(30);

        Assert.Equal(70, character.Health);
    }

    [Fact]
    public void TakeDamage_KillsCharacter_ReturnsTrue()
    {
        var character = new Character("player1", "Aragorn", "warrior") { Health = 50, MaxHealth = 100 };

        bool isDead = character.TakeDamage(50);

        Assert.True(isDead);
        Assert.Equal(0, character.Health);
    }

    [Fact]
    public void Heal_RestoredHealth()
    {
        var character = new Character("player1", "Aragorn", "warrior") { Health = 50, MaxHealth = 100 };

        character.Heal(30);

        Assert.Equal(80, character.Health);
    }

    [Fact]
    public void Heal_ExceedsMax_CapsAtMax()
    {
        var character = new Character("player1", "Aragorn", "warrior") { Health = 90, MaxHealth = 100 };

        character.Heal(30);

        Assert.Equal(100, character.Health);
    }

    [Fact]
    public void LearnSkill_NewSkill_Added()
    {
        var character = new Character("player1", "Aragorn", "warrior");

        character.LearnSkill("slash");

        Assert.Contains("slash", character.SkillIds);
    }

    [Fact]
    public void LearnSkill_DuplicateSkill_NotAdded()
    {
        var character = new Character("player1", "Aragorn", "warrior");
        character.LearnSkill("slash");

        character.LearnSkill("slash");

        Assert.Single(character.SkillIds);
    }

    [Fact]
    public void IsAlive_WithHealth_ReturnsTrue()
    {
        var character = new Character("player1", "Aragorn", "warrior") { Health = 50 };

        Assert.True(character.IsAlive());
    }

    [Fact]
    public void IsAlive_ZeroHealth_ReturnsFalse()
    {
        var character = new Character("player1", "Aragorn", "warrior") { Health = 0 };

        Assert.False(character.IsAlive());
    }
}
