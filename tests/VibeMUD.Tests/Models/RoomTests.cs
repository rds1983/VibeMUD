namespace VibeMUD.Tests.Models;

using VibeMUD.Models;

public class RoomTests
{
    [Fact]
    public void Constructor_WithParameters_InitializesCorrectly()
    {
        var room = new Room("city_center", "City Center", "A bustling city square");

        Assert.Equal("city_center", room.Id);
        Assert.Equal("City Center", room.Title);
        Assert.Equal("A bustling city square", room.Description);
    }

    [Fact]
    public void HasExit_ExistingExit_ReturnsTrue()
    {
        var room = new Room("city_center", "City Center", "desc");
        room.AddExit("north", "starter_city", "temple");

        Assert.True(room.HasExit("north"));
    }

    [Fact]
    public void HasExit_NonExistingExit_ReturnsFalse()
    {
        var room = new Room("city_center", "City Center", "desc");

        Assert.False(room.HasExit("north"));
    }

    [Fact]
    public void GetExit_ExistingExit_ReturnsExit()
    {
        var room = new Room("city_center", "City Center", "desc");
        room.AddExit("north", "starter_city", "temple");

        var exit = room.GetExit("north");

        Assert.NotNull(exit);
        Assert.Equal("starter_city", exit?.areaId);
        Assert.Equal("temple", exit?.roomId);
    }

    [Fact]
    public void GetExit_NonExistingExit_ReturnsNull()
    {
        var room = new Room("city_center", "City Center", "desc");

        var exit = room.GetExit("north");

        Assert.Null(exit);
    }

    [Fact]
    public void AddExit_NewExit_Added()
    {
        var room = new Room("city_center", "City Center", "desc");

        room.AddExit("south", "starter_city", "market");

        Assert.True(room.HasExit("south"));
    }

    [Fact]
    public void RemoveExit_ExistingExit_Removed()
    {
        var room = new Room("city_center", "City Center", "desc");
        room.AddExit("north", "starter_city", "temple");

        room.RemoveExit("north");

        Assert.False(room.HasExit("north"));
    }

    [Fact]
    public void AddNPC_NewNPC_Added()
    {
        var room = new Room("city_center", "City Center", "desc");

        room.AddNPC("guard_captain");

        Assert.Contains("guard_captain", room.NPCIds);
    }

    [Fact]
    public void AddNPC_DuplicateNPC_NotAdded()
    {
        var room = new Room("city_center", "City Center", "desc");
        room.AddNPC("guard_captain");

        room.AddNPC("guard_captain");

        Assert.Single(room.NPCIds);
    }

    [Fact]
    public void RemoveNPC_ExistingNPC_Removed()
    {
        var room = new Room("city_center", "City Center", "desc");
        room.AddNPC("guard_captain");

        room.RemoveNPC("guard_captain");

        Assert.DoesNotContain("guard_captain", room.NPCIds);
    }

    [Fact]
    public void AddItem_NewItem_Added()
    {
        var room = new Room("city_center", "City Center", "desc");
        var item = new Item("gold_coin", "Gold Coin", "consumable", "currency", 1);

        room.AddItem(item);

        Assert.Contains(item, room.Items);
    }

    [Fact]
    public void RemoveItem_ExistingItem_Removed()
    {
        var room = new Room("city_center", "City Center", "desc");
        var item = new Item("gold_coin", "Gold Coin", "consumable", "currency", 1);
        room.AddItem(item);

        room.RemoveItem(item);

        Assert.DoesNotContain(item, room.Items);
    }

    [Fact]
    public void AddPlayer_NewPlayer_Added()
    {
        var room = new Room("city_center", "City Center", "desc");

        room.AddPlayer("player1");

        Assert.Contains("player1", room.PlayerIds);
    }

    [Fact]
    public void AddPlayer_DuplicatePlayer_NotAdded()
    {
        var room = new Room("city_center", "City Center", "desc");
        room.AddPlayer("player1");

        room.AddPlayer("player1");

        Assert.Single(room.PlayerIds);
    }

    [Fact]
    public void RemovePlayer_ExistingPlayer_Removed()
    {
        var room = new Room("city_center", "City Center", "desc");
        room.AddPlayer("player1");

        room.RemovePlayer("player1");

        Assert.DoesNotContain("player1", room.PlayerIds);
    }

    [Fact]
    public void GetOccupants_ReturnsAllOccupants()
    {
        var room = new Room("city_center", "City Center", "desc");
        room.AddNPC("guard");
        room.AddNPC("merchant");
        room.AddPlayer("player1");

        var occupants = room.GetOccupants();

        Assert.Contains("guard", occupants);
        Assert.Contains("merchant", occupants);
        Assert.Contains("player1", occupants);
        Assert.Equal(3, occupants.Count);
    }

    [Fact]
    public void GetExitDescription_WithExits_ListsExits()
    {
        var room = new Room("city_center", "City Center", "desc");
        room.AddExit("north", "starter_city", "temple");
        room.AddExit("south", "starter_city", "market");

        var exitDesc = room.GetExitDescription();

        Assert.Contains("north", exitDesc);
        Assert.Contains("south", exitDesc);
    }

    [Fact]
    public void GetExitDescription_NoExits_ReturnsNoExits()
    {
        var room = new Room("city_center", "City Center", "desc");

        var exitDesc = room.GetExitDescription();

        Assert.Equal("No visible exits.", exitDesc);
    }
}
