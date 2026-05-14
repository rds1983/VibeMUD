namespace VibeMUD.Systems;

using VibeMUD.Models;

/// <summary>
/// Manages shop operations including buying, selling, and inventory management
/// </summary>
public class ShopSystem
{
    public class Shop
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShopkeeperId { get; set; } = string.Empty;
        public Dictionary<string, int> Inventory { get; set; } = new(); // itemId -> quantity
        public double BuySellMargin { get; set; } = 1.5; // Shop sells at 1.5x of purchase price
    }

    public class TransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int GoldChange { get; set; }
        public Item? Item { get; set; }
    }

    /// <summary>
    /// Attempts to buy an item from the shop
    /// </summary>
    public static TransactionResult BuyFromShop(
        Shop shop,
        Character character,
        Item item,
        int characterLevel)
    {
        var result = new TransactionResult();

        // Check if shop has the item
        if (!shop.Inventory.ContainsKey(item.Id) || shop.Inventory[item.Id] <= 0)
        {
            result.Success = false;
            result.Message = $"The {shop.Name} doesn't have {item.Name} in stock.";
            return result;
        }

        // Calculate price
        int price = LootSystem.BuyItemPrice(item, shop.BuySellMargin, characterLevel);

        // Check character has enough gold
        if (character.Gold < price)
        {
            result.Success = false;
            result.Message = $"You need {price} gold to buy {item.Name}, but only have {character.Gold}.";
            return result;
        }

        // Check inventory space
        if (!character.AddToInventory(item))
        {
            result.Success = false;
            result.Message = "Your inventory is too full to carry that item.";
            return result;
        }

        // Process transaction
        character.Gold -= price;
        shop.Inventory[item.Id]--;

        result.Success = true;
        result.GoldChange = -price;
        result.Item = item;
        result.Message = $"You buy {item.Name} for {price} gold.";

        return result;
    }

    /// <summary>
    /// Attempts to sell an item to the shop
    /// </summary>
    public static TransactionResult SellToShop(
        Shop shop,
        Character character,
        Item item,
        int characterLevel)
    {
        var result = new TransactionResult();

        // Check if player has the item
        if (!character.Inventory.Contains(item))
        {
            result.Success = false;
            result.Message = "You don't have that item to sell.";
            return result;
        }

        // Calculate sale price
        int salePrice = LootSystem.SellItemPrice(item, characterLevel);

        // Process transaction
        character.RemoveFromInventory(item);
        character.Gold += salePrice;

        // Add item to shop inventory
        if (!shop.Inventory.ContainsKey(item.Id))
            shop.Inventory[item.Id] = 0;
        shop.Inventory[item.Id]++;

        result.Success = true;
        result.GoldChange = salePrice;
        result.Item = item;
        result.Message = $"You sell {item.Name} for {salePrice} gold.";

        return result;
    }

    /// <summary>
    /// Gets the list of items available in a shop
    /// </summary>
    public static List<string> GetShopInventory(Shop shop)
    {
        return shop.Inventory
            .Where(kvp => kvp.Value > 0)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    /// <summary>
    /// Checks if a shop has a specific item in stock
    /// </summary>
    public static bool HasItem(Shop shop, string itemId)
    {
        return shop.Inventory.ContainsKey(itemId) && shop.Inventory[itemId] > 0;
    }

    /// <summary>
    /// Gets the quantity of an item in shop inventory
    /// </summary>
    public static int GetItemQuantity(Shop shop, string itemId)
    {
        if (!shop.Inventory.ContainsKey(itemId))
            return 0;
        return shop.Inventory[itemId];
    }

    /// <summary>
    /// Restocks a shop with items (for periodic restocking)
    /// </summary>
    public static void RestockItem(Shop shop, string itemId, int quantity)
    {
        if (!shop.Inventory.ContainsKey(itemId))
            shop.Inventory[itemId] = 0;
        shop.Inventory[itemId] += quantity;
    }

    /// <summary>
    /// Sets the stock quantity for an item
    /// </summary>
    public static void SetStock(Shop shop, string itemId, int quantity)
    {
        if (quantity < 0)
            quantity = 0;
        shop.Inventory[itemId] = quantity;
    }

    /// <summary>
    /// Gets the buying price (what character gets for selling)
    /// </summary>
    public static int GetSellingPrice(Item item, int characterLevel, double shopMargin)
    {
        // When selling, character gets base price (not margin-adjusted)
        return LootSystem.SellItemPrice(item, characterLevel);
    }

    /// <summary>
    /// Gets the selling price (what character pays for buying)
    /// </summary>
    public static int GetBuyingPrice(Item item, int characterLevel, double shopMargin)
    {
        return LootSystem.BuyItemPrice(item, shopMargin, characterLevel);
    }

    /// <summary>
    /// Calculates shop profit from a transaction
    /// </summary>
    public static int CalculateShopProfit(Item item, int characterLevel, double shopMargin)
    {
        int buyPrice = GetBuyingPrice(item, characterLevel, shopMargin);
        int sellPrice = GetSellingPrice(item, characterLevel, shopMargin);
        return buyPrice - sellPrice;
    }

    /// <summary>
    /// Validates shop configuration
    /// </summary>
    public static bool ValidateShop(Shop shop)
    {
        // Shop needs an ID and name
        if (string.IsNullOrEmpty(shop.Id) || string.IsNullOrEmpty(shop.Name))
            return false;

        // Margin should be > 1.0
        if (shop.BuySellMargin <= 1.0)
            return false;

        // All quantities should be non-negative
        foreach (var (itemId, quantity) in shop.Inventory)
        {
            if (string.IsNullOrEmpty(itemId) || quantity < 0)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets summary of shop inventory
    /// </summary>
    public static string GetInventorySummary(Shop shop, Dictionary<string, Item>? itemDatabase = null)
    {
        var items = new List<string>();

        foreach (var (itemId, quantity) in shop.Inventory)
        {
            if (quantity <= 0)
                continue;

            if (itemDatabase != null && itemDatabase.TryGetValue(itemId, out var item))
            {
                items.Add($"{item.Name} ({quantity})");
            }
            else
            {
                items.Add($"{itemId} ({quantity})");
            }
        }

        if (items.Count == 0)
            return $"{shop.Name} has no items in stock.";

        return $"{shop.Name} has: {string.Join(", ", items)}";
    }

    /// <summary>
    /// Attempts bulk transaction (buy/sell multiple items)
    /// </summary>
    public static (int successCount, int goldNet, string message) BulkTransaction(
        Shop shop,
        Character character,
        List<Item> items,
        bool isSelling,
        int characterLevel)
    {
        int successCount = 0;
        int goldNet = 0;
        var messages = new List<string>();

        foreach (var item in items)
        {
            var result = isSelling
                ? SellToShop(shop, character, item, characterLevel)
                : BuyFromShop(shop, character, item, characterLevel);

            if (result.Success)
            {
                successCount++;
                goldNet += result.GoldChange;
                messages.Add(result.Message);
            }
            else
            {
                messages.Add(result.Message);
            }
        }

        string message = successCount > 0
            ? $"Transactions complete. Net change: {goldNet} gold."
            : "No transactions completed.";

        return (successCount, goldNet, message);
    }
}
