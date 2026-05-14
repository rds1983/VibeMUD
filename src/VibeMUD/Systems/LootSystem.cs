namespace VibeMUD.Systems;

using VibeMUD.Models;

/// <summary>
/// Manages loot drops from defeated NPCs and item distribution
/// </summary>
public class LootSystem
{
    public enum ItemRarity
    {
        Common,      // 50%+ drop chance
        Uncommon,    // 25-50% drop chance
        Rare,        // 10-25% drop chance
        VeryRare,    // 2-10% drop chance
        Legendary    // <2% drop chance
    }

    public class LootDrop
    {
        public int GoldAmount { get; set; }
        public List<Item> Items { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generates loot from a defeated NPC
    /// </summary>
    public static LootDrop GenerateLoot(NPC npc)
    {
        var loot = new LootDrop();
        var random = new Random();

        // Generate gold loot
        int goldDropped = npc.GenerateLoot();
        loot.GoldAmount = goldDropped;

        // Generate item drops
        var droppedItemIds = npc.RollLootItems();
        var itemMessages = new List<string>();

        if (droppedItemIds.Count > 0)
        {
            itemMessages.Add($"The {npc.Name} drops:");
            foreach (var itemId in droppedItemIds)
            {
                itemMessages.Add($"  - {itemId}");
            }
        }

        // Build message
        var messageLines = new List<string>();
        if (goldDropped > 0)
        {
            messageLines.Add($"You loot {goldDropped} gold from the corpse.");
        }
        messageLines.AddRange(itemMessages);

        loot.Message = string.Join(" ", messageLines);

        return loot;
    }

    /// <summary>
    /// Determines the rarity of an item based on drop chance
    /// </summary>
    public static ItemRarity DetermineRarity(double dropChance)
    {
        if (dropChance >= 0.5)
            return ItemRarity.Common;
        if (dropChance >= 0.25)
            return ItemRarity.Uncommon;
        if (dropChance >= 0.1)
            return ItemRarity.Rare;
        if (dropChance >= 0.02)
            return ItemRarity.VeryRare;
        return ItemRarity.Legendary;
    }

    /// <summary>
    /// Calculates item value based on required level and base value
    /// </summary>
    public static int CalculateItemValue(Item item, int characterLevel = 1)
    {
        if (item.Value <= 0)
            return 0;

        // Item value scales based on character level relative to item level
        // Higher character level relative to item = item is cheaper (you've outleveled it)
        // Lower character level relative to item = item is expensive (you need it)
        int levelDifference = item.RequiredLevel - characterLevel;

        // Value scales: +5% per level below character (more expensive when needed)
        // -5% per level above character (cheaper when outleveled)
        double levelMultiplier = 1.0 + (levelDifference * 0.05);
        levelMultiplier = Math.Max(0.5, Math.Min(2.0, levelMultiplier)); // Clamp between 0.5x and 2.0x

        return (int)(item.Value * levelMultiplier);
    }

    /// <summary>
    /// Sells an item to a shop (character receives gold)
    /// </summary>
    public static int SellItemPrice(Item item, int characterLevel = 1)
    {
        return CalculateItemValue(item, characterLevel);
    }

    /// <summary>
    /// Buys an item from a shop (character pays gold)
    /// </summary>
    public static int BuyItemPrice(Item item, double shopMargin = 1.5, int characterLevel = 1)
    {
        int basePrice = CalculateItemValue(item, characterLevel);
        return (int)(basePrice * shopMargin);
    }

    /// <summary>
    /// Generates loot drops for an NPC with item lookups
    /// </summary>
    public static LootDrop GenerateLootWithItems(NPC npc, Dictionary<string, Item> itemDatabase)
    {
        var loot = new LootDrop();

        // Generate gold
        loot.GoldAmount = npc.GenerateLoot();

        // Generate items
        var droppedItemIds = npc.RollLootItems();
        var messageLines = new List<string>();

        if (loot.GoldAmount > 0)
        {
            messageLines.Add($"You loot {loot.GoldAmount} gold.");
        }

        foreach (var itemId in droppedItemIds)
        {
            if (itemDatabase.TryGetValue(itemId, out var item))
            {
                loot.Items.Add(item);
                messageLines.Add($"You obtain: {item.Name}");
            }
        }

        loot.Message = messageLines.Count > 0
            ? string.Join(" ", messageLines)
            : $"The {npc.Name} had no loot.";

        return loot;
    }

    /// <summary>
    /// Validates loot table configuration
    /// </summary>
    public static bool ValidateLootTable(NPC npc)
    {
        // Gold range should be valid
        if (npc.GoldMin < 0 || npc.GoldMax < 0)
            return false;

        // Max should be >= Min
        if (npc.GoldMax < npc.GoldMin)
            return false;

        // Loot probabilities should be between 0 and 1
        foreach (var (itemId, probability) in npc.LootTable)
        {
            if (probability < 0 || probability > 1)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Calculates total value of all items in a list
    /// </summary>
    public static int CalculateTotalValue(List<Item> items, int characterLevel = 1)
    {
        return items.Sum(item => CalculateItemValue(item, characterLevel));
    }
}
