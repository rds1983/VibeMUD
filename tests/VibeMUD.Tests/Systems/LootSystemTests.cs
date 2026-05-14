namespace VibeMUD.Tests.Systems;

using VibeMUD.Models;
using VibeMUD.Systems;

public class LootSystemTests
{
    private Item CreateTestItem(string id = "sword1", int value = 100, int level = 1)
    {
        var item = new Item(id, "Test Item", "weapon", "sword", value, level);
        item.Weight = 2.0;
        return item;
    }

    [Fact]
    public void GenerateLoot_ReturnsGoldInRange()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            GoldMin = 10,
            GoldMax = 50
        };

        var loot = LootSystem.GenerateLoot(npc);

        Assert.InRange(loot.GoldAmount, 10, 50);
    }

    [Fact]
    public void GenerateLoot_WithNoItems_OnlyReturnsGold()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            GoldMin = 20,
            GoldMax = 40,
            LootTable = new Dictionary<string, double>()
        };

        var loot = LootSystem.GenerateLoot(npc);

        Assert.True(loot.GoldAmount >= 20);
        Assert.Empty(loot.Items);
    }

    [Fact]
    public void GenerateLoot_CreatesValidMessage()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            GoldMin = 5,
            GoldMax = 15
        };

        var loot = LootSystem.GenerateLoot(npc);

        Assert.NotEmpty(loot.Message);
        Assert.Contains("gold", loot.Message.ToLower());
    }

    [Fact]
    public void DetermineRarity_HighChance_IsCommon()
    {
        var rarity = LootSystem.DetermineRarity(0.75);
        Assert.Equal(LootSystem.ItemRarity.Common, rarity);
    }

    [Fact]
    public void DetermineRarity_MidHighChance_IsUncommon()
    {
        var rarity = LootSystem.DetermineRarity(0.35);
        Assert.Equal(LootSystem.ItemRarity.Uncommon, rarity);
    }

    [Fact]
    public void DetermineRarity_MidLowChance_IsRare()
    {
        var rarity = LootSystem.DetermineRarity(0.15);
        Assert.Equal(LootSystem.ItemRarity.Rare, rarity);
    }

    [Fact]
    public void DetermineRarity_LowChance_IsVeryRare()
    {
        var rarity = LootSystem.DetermineRarity(0.05);
        Assert.Equal(LootSystem.ItemRarity.VeryRare, rarity);
    }

    [Fact]
    public void DetermineRarity_VeryLowChance_IsLegendary()
    {
        var rarity = LootSystem.DetermineRarity(0.01);
        Assert.Equal(LootSystem.ItemRarity.Legendary, rarity);
    }

    [Fact]
    public void CalculateItemValue_BaseValue()
    {
        var item = CreateTestItem("sword", 100, 1);

        int value = LootSystem.CalculateItemValue(item, 1);

        Assert.Equal(100, value);
    }

    [Fact]
    public void CalculateItemValue_HigherLevel_MoreExpensive()
    {
        var item = CreateTestItem("sword", 100, 10);

        int value1 = LootSystem.CalculateItemValue(item, 1);   // Need it badly
        int value10 = LootSystem.CalculateItemValue(item, 10); // Perfect level
        int value15 = LootSystem.CalculateItemValue(item, 15); // Outleveled it

        // Items are more expensive when you need them (below your level)
        Assert.True(value1 > value10);   // Below level = more expensive
        Assert.True(value10 > value15);  // Above level = cheaper
    }

    [Fact]
    public void CalculateItemValue_LowerLevelItemCheaper()
    {
        var item = CreateTestItem("sword", 100, 1);

        int value1 = LootSystem.CalculateItemValue(item, 1);
        int value10 = LootSystem.CalculateItemValue(item, 10);

        Assert.True(value1 > value10);
    }

    [Fact]
    public void CalculateItemValue_Clamped()
    {
        var item = CreateTestItem("sword", 100, 1);

        // Very high character level should clamp to 2x
        int valueHighLevel = LootSystem.CalculateItemValue(item, 50);
        Assert.True(valueHighLevel <= 200);

        // Very low should clamp to 0.5x
        int valueLowLevel = LootSystem.CalculateItemValue(item, 1);
        Assert.True(valueLowLevel >= 50);
    }

    [Fact]
    public void SellItemPrice_ReturnsCorrectValue()
    {
        var item = CreateTestItem("sword", 100, 1);

        int price = LootSystem.SellItemPrice(item, 1);

        Assert.Equal(100, price);
    }

    [Fact]
    public void BuyItemPrice_AppliesMargin()
    {
        var item = CreateTestItem("sword", 100, 1);

        int buyPrice = LootSystem.BuyItemPrice(item, 1.5, 1);

        Assert.Equal(150, buyPrice);
    }

    [Fact]
    public void BuyItemPrice_DifferentMargin()
    {
        var item = CreateTestItem("sword", 100, 1);

        int buyPrice2x = LootSystem.BuyItemPrice(item, 2.0, 1);

        Assert.Equal(200, buyPrice2x);
    }

    [Fact]
    public void ValidateLootTable_ValidNPC_ReturnsTrue()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            GoldMin = 10,
            GoldMax = 50,
            LootTable = new Dictionary<string, double>
            {
                ["item1"] = 0.5,
                ["item2"] = 0.25
            }
        };

        bool isValid = LootSystem.ValidateLootTable(npc);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateLootTable_NegativeGold_ReturnsFalse()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            GoldMin = -10,
            GoldMax = 50
        };

        bool isValid = LootSystem.ValidateLootTable(npc);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateLootTable_MaxLessThanMin_ReturnsFalse()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            GoldMin = 50,
            GoldMax = 10
        };

        bool isValid = LootSystem.ValidateLootTable(npc);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateLootTable_InvalidProbability_ReturnsFalse()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            GoldMin = 10,
            GoldMax = 50,
            LootTable = new Dictionary<string, double>
            {
                ["item1"] = 1.5
            }
        };

        bool isValid = LootSystem.ValidateLootTable(npc);

        Assert.False(isValid);
    }

    [Fact]
    public void CalculateTotalValue_SumCorrect()
    {
        var items = new List<Item>
        {
            CreateTestItem("sword", 100, 1),
            CreateTestItem("armor", 50, 1)
        };

        int totalValue = LootSystem.CalculateTotalValue(items, 1);

        Assert.Equal(150, totalValue);
    }

    [Fact]
    public void CalculateTotalValue_Empty()
    {
        var items = new List<Item>();

        int totalValue = LootSystem.CalculateTotalValue(items);

        Assert.Equal(0, totalValue);
    }

    [Fact]
    public void GenerateLootWithItems_IncludesItemObjects()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            GoldMin = 10,
            GoldMax = 20,
            LootTable = new Dictionary<string, double>
            {
                ["sword1"] = 1.0
            }
        };

        var itemDb = new Dictionary<string, Item>
        {
            ["sword1"] = CreateTestItem("sword1", 50, 1)
        };

        var loot = LootSystem.GenerateLootWithItems(npc, itemDb);

        Assert.NotEmpty(loot.Items);
        Assert.Equal("sword1", loot.Items[0].Id);
    }

    [Fact]
    public void GenerateLootWithItems_UnknownItemIgnored()
    {
        var npc = new NPC("goblin", "Goblin", "desc", 3)
        {
            GoldMin = 10,
            GoldMax = 20,
            LootTable = new Dictionary<string, double>
            {
                ["unknown_item"] = 1.0
            }
        };

        var itemDb = new Dictionary<string, Item>();

        var loot = LootSystem.GenerateLootWithItems(npc, itemDb);

        Assert.Empty(loot.Items);
    }

    [Fact]
    public void GenerateLoot_GoldZero_HandledCorrectly()
    {
        var npc = new NPC("poor", "Poor Creature", "desc", 1)
        {
            GoldMin = 0,
            GoldMax = 0
        };

        var loot = LootSystem.GenerateLoot(npc);

        Assert.Equal(0, loot.GoldAmount);
    }

    [Fact]
    public void CalculateItemValue_ZeroValue_ReturnsZero()
    {
        var item = new Item("worthless", "Worthless Item", "junk", "none", 0, 1);

        int value = LootSystem.CalculateItemValue(item, 1);

        Assert.Equal(0, value);
    }
}
