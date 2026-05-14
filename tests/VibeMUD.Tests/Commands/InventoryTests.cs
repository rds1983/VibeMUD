namespace VibeMUD.Tests.Commands;

using VibeMUD.Commands;
using VibeMUD.Core;
using VibeMUD.Models;

public class InventoryTests
{
    private GameState CreateTestGameState()
    {
        var gameState = new GameState();
        var area = new Area("test_area", "Test Area", "A test area", 1, 10);
        var room = new Room("test_room", "Test Room", "A test room");
        area.AddRoom(room);
        gameState.Areas["test_area"] = area;
        return gameState;
    }

    [Fact]
    public void InventoryCommand_Empty_ShowsEmpty()
    {
        var character = new Character("player1", "Player", "warrior");
        var gameState = CreateTestGameState();
        var cmd = new InventoryCommand();

        var result = cmd.Execute(character, gameState, new string[] { });

        Assert.True(result.Success);
        Assert.Contains("empty", result.Message);
    }

    [Fact]
    public void InventoryCommand_WithItems_ShowsItems()
    {
        var character = new Character("player1", "Player", "warrior");
        var item = new Item("sword", "Iron Sword", "weapon", "sword", 50);
        character.AddToInventory(item);
        var gameState = CreateTestGameState();
        var cmd = new InventoryCommand();

        var result = cmd.Execute(character, gameState, new string[] { });

        Assert.True(result.Success);
        Assert.Contains("Iron Sword", result.Message);
    }

    [Fact]
    public void EquipCommand_ValidItem_Equips()
    {
        var character = new Character("player1", "Player", "warrior") { Level = 1 };
        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50, 1);
        item.Slot = "main-hand";
        character.AddToInventory(item);
        var gameState = CreateTestGameState();
        var cmd = new EquipCommand();

        var result = cmd.Execute(character, gameState, new[] { "Iron Sword" });

        Assert.True(result.Success);
        Assert.Equal(item, character.Equipment["main-hand"]);
        Assert.DoesNotContain(item, character.Inventory);
    }

    [Fact]
    public void EquipCommand_NonExistentItem_Fails()
    {
        var character = new Character("player1", "Player", "warrior");
        var gameState = CreateTestGameState();
        var cmd = new EquipCommand();

        var result = cmd.Execute(character, gameState, new[] { "nonexistent" });

        Assert.False(result.Success);
        Assert.Contains("don't have", result.Message);
    }

    [Fact]
    public void EquipCommand_InsufficientLevel_Fails()
    {
        var character = new Character("player1", "Player", "warrior") { Level = 5 };
        var item = new Item("legendary_sword", "Legendary Sword", "weapon", "sword", 500, 50);
        item.Slot = "main-hand";
        character.AddToInventory(item);
        var gameState = CreateTestGameState();
        var cmd = new EquipCommand();

        var result = cmd.Execute(character, gameState, new[] { "legendary" });

        Assert.False(result.Success);
        Assert.Contains("cannot equip", result.Message);
    }

    [Fact]
    public void UnequipCommand_EquippedItem_Unequips()
    {
        var character = new Character("player1", "Player", "warrior") { Level = 1 };
        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50, 1);
        item.Slot = "main-hand";
        character.Equipment["main-hand"] = item;
        var gameState = CreateTestGameState();
        var cmd = new UnequipCommand();

        var result = cmd.Execute(character, gameState, new[] { "main-hand" });

        Assert.True(result.Success);
        Assert.Null(character.Equipment["main-hand"]);
        Assert.Contains(item, character.Inventory);
    }

    [Fact]
    public void UnequipCommand_EmptySlot_Fails()
    {
        var character = new Character("player1", "Player", "warrior");
        var gameState = CreateTestGameState();
        var cmd = new UnequipCommand();

        var result = cmd.Execute(character, gameState, new[] { "main-hand" });

        Assert.False(result.Success);
        Assert.Contains("Nothing equipped", result.Message);
    }

    [Fact]
    public void UnequipCommand_FullInventory_Fails()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            Level = 1,
            MaxInventoryWeight = 0.1
        };
        var item = new Item("iron_sword", "Iron Sword", "weapon", "sword", 50, 1);
        item.Slot = "main-hand";
        item.Weight = 3.5;
        character.Equipment["main-hand"] = item;
        var gameState = CreateTestGameState();
        var cmd = new UnequipCommand();

        var result = cmd.Execute(character, gameState, new[] { "main-hand" });

        Assert.False(result.Success);
        Assert.Contains("inventory is full", result.Message);
        Assert.Equal(item, character.Equipment["main-hand"]);
    }

    [Fact]
    public void DropCommand_InventoryItem_Drops()
    {
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "test_room";
        var item = new Item("gold_coin", "Gold Coin", "consumable", "currency", 1);
        character.AddToInventory(item);
        var gameState = CreateTestGameState();
        var cmd = new DropCommand();

        var result = cmd.Execute(character, gameState, new[] { "Gold Coin" });

        Assert.True(result.Success);
        Assert.DoesNotContain(item, character.Inventory);
        var room = gameState.GetRoom("test_area", "test_room");
        Assert.Contains(item, room!.Items);
    }

    [Fact]
    public void DropCommand_NonExistentItem_Fails()
    {
        var character = new Character("player1", "Player", "warrior");
        var gameState = CreateTestGameState();
        var cmd = new DropCommand();

        var result = cmd.Execute(character, gameState, new[] { "nonexistent" });

        Assert.False(result.Success);
        Assert.Contains("don't have", result.Message);
    }

    [Fact]
    public void TakeCommand_RoomItem_Picks()
    {
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "test_room";
        var item = new Item("gold_coin", "Gold Coin", "consumable", "currency", 1);
        var gameState = CreateTestGameState();
        gameState.Areas["test_area"].GetRoom("test_room")?.AddItem(item);
        var cmd = new TakeCommand();

        var result = cmd.Execute(character, gameState, new[] { "Gold Coin" });

        Assert.True(result.Success);
        Assert.Contains(item, character.Inventory);
    }

    [Fact]
    public void TakeCommand_NonExistentItem_Fails()
    {
        var character = new Character("player1", "Player", "warrior");
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "test_room";
        var gameState = CreateTestGameState();
        var cmd = new TakeCommand();

        var result = cmd.Execute(character, gameState, new[] { "nonexistent" });

        Assert.False(result.Success);
        Assert.Contains("don't see", result.Message);
    }

    [Fact]
    public void TakeCommand_FullInventory_Fails()
    {
        var character = new Character("player1", "Player", "warrior")
        {
            MaxInventoryWeight = 0.1
        };
        character.CurrentAreaId = "test_area";
        character.CurrentRoomId = "test_room";
        var item = new Item("heavy_item", "Heavy Item", "equipment", "misc", 50);
        item.Weight = 10.0;
        var gameState = CreateTestGameState();
        gameState.Areas["test_area"].GetRoom("test_room")?.AddItem(item);
        var cmd = new TakeCommand();

        var result = cmd.Execute(character, gameState, new[] { "Heavy Item" });

        Assert.False(result.Success);
        Assert.Contains("inventory is full", result.Message);
    }
}
