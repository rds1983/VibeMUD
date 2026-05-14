namespace VibeMUD.Tests.Models;

using VibeMUD.Models;

public class ItemTests
{
    [Fact]
    public void Constructor_WithBasicParameters_InitializesCorrectly()
    {
        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50, 1);

        Assert.Equal("iron_sword", item.Id);
        Assert.Equal("Iron Sword", item.Name);
        Assert.Equal("weapon", item.Type);
        Assert.Equal("sword", item.Subtype);
        Assert.Equal(50, item.Value);
        Assert.Equal(1, item.RequiredLevel);
    }

    [Fact]
    public void GetDamage_WithDamageInStats_ReturnsDamage()
    {
        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50);
        item.Stats["damage"] = 8;

        Assert.Equal(8, item.GetDamage());
    }

    [Fact]
    public void GetDamage_WithoutDamageInStats_ReturnsZero()
    {
        var item = new Item("leather_armor", "Leather Armor", "armor", "chest", 30);

        Assert.Equal(0, item.GetDamage());
    }

    [Fact]
    public void GetArmor_WithArmorInStats_ReturnsArmor()
    {
        var item = new Item("leather_armor", "Leather Armor", "armor", "chest", 30);
        item.Stats["armor"] = 5;

        Assert.Equal(5, item.GetArmor());
    }

    [Fact]
    public void GetStatBonus_WithBonus_ReturnsBonus()
    {
        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50);
        item.Stats["bonus_strength"] = 2;

        Assert.Equal(2, item.GetStatBonus("strength"));
    }

    [Fact]
    public void CanEquip_WithSufficientLevel_ReturnsTrue()
    {
        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50, 1);

        Assert.True(item.CanEquip(5));
    }

    [Fact]
    public void CanEquip_WithInsufficientLevel_ReturnsFalse()
    {
        var item = new Item("legendary_sword", "Legendary Sword", "weapon", "sword", 500, 50);

        Assert.False(item.CanEquip(40));
    }

    [Fact]
    public void CanEquip_WithExactLevel_ReturnsTrue()
    {
        var item = new Item("epic_bow", "Epic Bow", "weapon", "bow", 200, 20);

        Assert.True(item.CanEquip(20));
    }
}
