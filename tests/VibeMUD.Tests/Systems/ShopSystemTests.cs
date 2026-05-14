namespace VibeMUD.Tests.Systems;

using VibeMUD.Models;
using VibeMUD.Systems;

public class ShopSystemTests
{
    private ShopSystem.Shop CreateTestShop(string id = "shop1")
    {
        return new ShopSystem.Shop
        {
            Id = id,
            Name = "Test Shop",
            ShopkeeperId = "shopkeeper1",
            BuySellMargin = 1.5,
            Inventory = new Dictionary<string, int>
            {
                ["sword1"] = 5,
                ["armor1"] = 3
            }
        };
    }

    private Item CreateTestItem(string id = "sword1", int value = 100, int level = 1)
    {
        var item = new Item(id, "Test Item", "weapon", "sword", value, level);
        item.Weight = 2.0;
        return item;
    }

    [Fact]
    public void BuyFromShop_ValidTransaction_Success()
    {
        var shop = CreateTestShop();
        var character = new Character("c1", "Hero", "warrior")
        {
            Gold = 200
        };
        var item = CreateTestItem();

        var result = ShopSystem.BuyFromShop(shop, character, item, 1);

        Assert.True(result.Success);
        Assert.Equal(-150, result.GoldChange);
        Assert.Equal(50, character.Gold);
    }

    [Fact]
    public void BuyFromShop_ItemNotInStock_Fails()
    {
        var shop = CreateTestShop();
        shop.Inventory["sword1"] = 0;
        var character = new Character("c1", "Hero", "warrior")
        {
            Gold = 200
        };
        var item = CreateTestItem();

        var result = ShopSystem.BuyFromShop(shop, character, item, 1);

        Assert.False(result.Success);
        Assert.Contains("stock", result.Message.ToLower());
    }

    [Fact]
    public void BuyFromShop_InsufficientGold_Fails()
    {
        var shop = CreateTestShop();
        var character = new Character("c1", "Hero", "warrior")
        {
            Gold = 100
        };
        var item = CreateTestItem();

        var result = ShopSystem.BuyFromShop(shop, character, item, 1);

        Assert.False(result.Success);
        Assert.Contains("gold", result.Message.ToLower());
    }

    [Fact]
    public void BuyFromShop_InventoryFull_Fails()
    {
        var shop = CreateTestShop();
        var character = new Character("c1", "Hero", "warrior")
        {
            Gold = 200,
            InventoryWeight = 100.0,
            MaxInventoryWeight = 100.0
        };
        var item = CreateTestItem("sword1", 100);

        var result = ShopSystem.BuyFromShop(shop, character, item, 1);

        Assert.False(result.Success);
        Assert.Contains("full", result.Message.ToLower());
    }

    [Fact]
    public void BuyFromShop_ReducesShopInventory()
    {
        var shop = CreateTestShop();
        var character = new Character("c1", "Hero", "warrior")
        {
            Gold = 200
        };
        var item = CreateTestItem();

        int initialQuantity = shop.Inventory["sword1"];
        ShopSystem.BuyFromShop(shop, character, item, 1);

        Assert.Equal(initialQuantity - 1, shop.Inventory["sword1"]);
    }

    [Fact]
    public void SellToShop_ValidTransaction_Success()
    {
        var shop = CreateTestShop();
        var character = new Character("c1", "Hero", "warrior")
        {
            Gold = 0
        };
        var item = CreateTestItem();
        character.AddToInventory(item);

        var result = ShopSystem.SellToShop(shop, character, item, 1);

        Assert.True(result.Success);
        Assert.Equal(100, result.GoldChange);
        Assert.Equal(100, character.Gold);
    }

    [Fact]
    public void SellToShop_ItemNotOwned_Fails()
    {
        var shop = CreateTestShop();
        var character = new Character("c1", "Hero", "warrior");
        var item = CreateTestItem();

        var result = ShopSystem.SellToShop(shop, character, item, 1);

        Assert.False(result.Success);
        Assert.Contains("don't have", result.Message);
    }

    [Fact]
    public void SellToShop_IncreasesShopInventory()
    {
        var shop = CreateTestShop();
        var character = new Character("c1", "Hero", "warrior");
        var item = CreateTestItem("newitem");
        shop.Inventory["newitem"] = 0;
        character.AddToInventory(item);

        ShopSystem.SellToShop(shop, character, item, 1);

        Assert.Equal(1, shop.Inventory["newitem"]);
    }

    [Fact]
    public void SellToShop_RemovesFromCharacterInventory()
    {
        var shop = CreateTestShop();
        var character = new Character("c1", "Hero", "warrior");
        var item = CreateTestItem();
        character.AddToInventory(item);

        ShopSystem.SellToShop(shop, character, item, 1);

        Assert.DoesNotContain(item, character.Inventory);
    }

    [Fact]
    public void GetShopInventory_ReturnsAvailableItems()
    {
        var shop = CreateTestShop();
        shop.Inventory["sword1"] = 5;
        shop.Inventory["armor1"] = 0;

        var inventory = ShopSystem.GetShopInventory(shop);

        Assert.Contains("sword1", inventory);
        Assert.DoesNotContain("armor1", inventory);
    }

    [Fact]
    public void HasItem_ItemInStock_ReturnsTrue()
    {
        var shop = CreateTestShop();

        bool hasItem = ShopSystem.HasItem(shop, "sword1");

        Assert.True(hasItem);
    }

    [Fact]
    public void HasItem_ItemOutOfStock_ReturnsFalse()
    {
        var shop = CreateTestShop();
        shop.Inventory["sword1"] = 0;

        bool hasItem = ShopSystem.HasItem(shop, "sword1");

        Assert.False(hasItem);
    }

    [Fact]
    public void HasItem_ItemNotExists_ReturnsFalse()
    {
        var shop = CreateTestShop();

        bool hasItem = ShopSystem.HasItem(shop, "nonexistent");

        Assert.False(hasItem);
    }

    [Fact]
    public void GetItemQuantity_ReturnsCorrectQuantity()
    {
        var shop = CreateTestShop();

        int quantity = ShopSystem.GetItemQuantity(shop, "sword1");

        Assert.Equal(5, quantity);
    }

    [Fact]
    public void GetItemQuantity_NotInInventory_ReturnsZero()
    {
        var shop = CreateTestShop();

        int quantity = ShopSystem.GetItemQuantity(shop, "nonexistent");

        Assert.Equal(0, quantity);
    }

    [Fact]
    public void RestockItem_IncreasesQuantity()
    {
        var shop = CreateTestShop();
        int initialQuantity = shop.Inventory["sword1"];

        ShopSystem.RestockItem(shop, "sword1", 3);

        Assert.Equal(initialQuantity + 3, shop.Inventory["sword1"]);
    }

    [Fact]
    public void RestockItem_NewItem_AddsItem()
    {
        var shop = CreateTestShop();

        ShopSystem.RestockItem(shop, "newitem", 5);

        Assert.Equal(5, shop.Inventory["newitem"]);
    }

    [Fact]
    public void SetStock_SetsQuantity()
    {
        var shop = CreateTestShop();

        ShopSystem.SetStock(shop, "sword1", 10);

        Assert.Equal(10, shop.Inventory["sword1"]);
    }

    [Fact]
    public void SetStock_NegativeQuantity_ClampsToZero()
    {
        var shop = CreateTestShop();

        ShopSystem.SetStock(shop, "sword1", -5);

        Assert.Equal(0, shop.Inventory["sword1"]);
    }

    [Fact]
    public void GetSellingPrice_ReturnsBasePrice()
    {
        var item = CreateTestItem("sword", 100);

        int price = ShopSystem.GetSellingPrice(item, 1, 1.5);

        Assert.Equal(100, price);
    }

    [Fact]
    public void GetBuyingPrice_AppliesMargin()
    {
        var item = CreateTestItem("sword", 100);

        int price = ShopSystem.GetBuyingPrice(item, 1, 1.5);

        Assert.Equal(150, price);
    }

    [Fact]
    public void CalculateShopProfit_ReturnsCorrectDifference()
    {
        var item = CreateTestItem("sword", 100);

        int profit = ShopSystem.CalculateShopProfit(item, 1, 1.5);

        Assert.Equal(50, profit);
    }

    [Fact]
    public void ValidateShop_ValidShop_ReturnsTrue()
    {
        var shop = CreateTestShop();

        bool isValid = ShopSystem.ValidateShop(shop);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateShop_NoId_ReturnsFalse()
    {
        var shop = CreateTestShop();
        shop.Id = "";

        bool isValid = ShopSystem.ValidateShop(shop);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateShop_NoName_ReturnsFalse()
    {
        var shop = CreateTestShop();
        shop.Name = "";

        bool isValid = ShopSystem.ValidateShop(shop);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateShop_InvalidMargin_ReturnsFalse()
    {
        var shop = CreateTestShop();
        shop.BuySellMargin = 0.9;

        bool isValid = ShopSystem.ValidateShop(shop);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateShop_NegativeQuantity_ReturnsFalse()
    {
        var shop = CreateTestShop();
        shop.Inventory["sword1"] = -5;

        bool isValid = ShopSystem.ValidateShop(shop);

        Assert.False(isValid);
    }

    [Fact]
    public void GetInventorySummary_WithItems()
    {
        var shop = CreateTestShop();
        var itemDb = new Dictionary<string, Item>
        {
            ["sword1"] = CreateTestItem("sword1"),
            ["armor1"] = CreateTestItem("armor1")
        };

        string summary = ShopSystem.GetInventorySummary(shop, itemDb);

        Assert.Contains("Test Shop", summary);
    }

    [Fact]
    public void GetInventorySummary_Empty()
    {
        var shop = CreateTestShop();
        shop.Inventory.Clear();

        string summary = ShopSystem.GetInventorySummary(shop);

        Assert.Contains("no items", summary.ToLower());
    }

    [Fact]
    public void BulkTransaction_BuyMultipleItems()
    {
        var shop = CreateTestShop();
        shop.Inventory["item1"] = 5;
        shop.Inventory["item2"] = 5;

        var character = new Character("c1", "Hero", "warrior")
        {
            Gold = 1000
        };

        var items = new List<Item>
        {
            CreateTestItem("item1", 100),
            CreateTestItem("item2", 100)
        };

        var (successCount, goldNet, message) = ShopSystem.BulkTransaction(shop, character, items, false, 1);

        Assert.Equal(2, successCount);
        Assert.True(goldNet < 0);
    }

    [Fact]
    public void BulkTransaction_SellMultipleItems()
    {
        var shop = CreateTestShop();
        var character = new Character("c1", "Hero", "warrior")
        {
            Gold = 0
        };

        var item1 = CreateTestItem("item1", 100);
        var item2 = CreateTestItem("item2", 100);
        character.AddToInventory(item1);
        character.AddToInventory(item2);

        var items = new List<Item> { item1, item2 };

        var (successCount, goldNet, message) = ShopSystem.BulkTransaction(shop, character, items, true, 1);

        Assert.Equal(2, successCount);
        Assert.True(goldNet > 0);
    }

    [Fact]
    public void TransactionResult_HasCorrectProperties()
    {
        var result = new ShopSystem.TransactionResult
        {
            Success = true,
            Message = "Transaction successful",
            GoldChange = 100,
            Item = CreateTestItem()
        };

        Assert.True(result.Success);
        Assert.Equal(100, result.GoldChange);
        Assert.NotNull(result.Item);
    }

    [Fact]
    public void Shop_DefaultMargin_1_5()
    {
        var shop = new ShopSystem.Shop();

        Assert.Equal(1.5, shop.BuySellMargin);
    }
}
