namespace VibeMUD.Tests.Models;

using VibeMUD.Models;

public class AreaTests
{
    [Fact]
    public void Constructor_WithParameters_InitializesCorrectly()
    {
        var area = new Area("starter_city", "Starter City", "A welcoming city for new adventurers", 1, 10);

        Assert.Equal("starter_city", area.Id);
        Assert.Equal("Starter City", area.Name);
        Assert.Equal("A welcoming city for new adventurers", area.Description);
        Assert.Equal(1, area.MinLevel);
        Assert.Equal(10, area.MaxLevel);
    }

    [Fact]
    public void AddRoom_NewRoom_Added()
    {
        var area = new Area("starter_city", "Starter City", "desc", 1, 10);
        var room = new Room("city_center", "City Center", "desc");

        area.AddRoom(room);

        Assert.True(area.RoomExists("city_center"));
    }

    [Fact]
    public void AddRoom_DuplicateRoom_NotDuplicated()
    {
        var area = new Area("starter_city", "Starter City", "desc", 1, 10);
        var room1 = new Room("city_center", "City Center", "desc");
        var room2 = new Room("city_center", "City Center", "different desc");

        area.AddRoom(room1);
        area.AddRoom(room2);

        Assert.Single(area.Rooms);
        Assert.Equal("desc", area.GetRoom("city_center")?.Description);
    }

    [Fact]
    public void GetRoom_ExistingRoom_ReturnsRoom()
    {
        var area = new Area("starter_city", "Starter City", "desc", 1, 10);
        var room = new Room("city_center", "City Center", "A bustling city square");
        area.AddRoom(room);

        var retrievedRoom = area.GetRoom("city_center");

        Assert.NotNull(retrievedRoom);
        Assert.Equal("city_center", retrievedRoom?.Id);
        Assert.Equal("A bustling city square", retrievedRoom?.Description);
    }

    [Fact]
    public void GetRoom_NonExistingRoom_ReturnsNull()
    {
        var area = new Area("starter_city", "Starter City", "desc", 1, 10);

        var room = area.GetRoom("nonexistent");

        Assert.Null(room);
    }

    [Fact]
    public void RoomExists_ExistingRoom_ReturnsTrue()
    {
        var area = new Area("starter_city", "Starter City", "desc", 1, 10);
        var room = new Room("city_center", "City Center", "desc");
        area.AddRoom(room);

        Assert.True(area.RoomExists("city_center"));
    }

    [Fact]
    public void RoomExists_NonExistingRoom_ReturnsFalse()
    {
        var area = new Area("starter_city", "Starter City", "desc", 1, 10);

        Assert.False(area.RoomExists("nonexistent"));
    }

    [Fact]
    public void GetRoomCount_ReturnsCorrectCount()
    {
        var area = new Area("starter_city", "Starter City", "desc", 1, 10);
        area.AddRoom(new Room("city_center", "City Center", "desc"));
        area.AddRoom(new Room("market", "Market", "desc"));
        area.AddRoom(new Room("inn", "Inn", "desc"));

        Assert.Equal(3, area.GetRoomCount());
    }

    [Fact]
    public void IsValidForLevel_WithinRange_ReturnsTrue()
    {
        var area = new Area("starter_city", "Starter City", "desc", 1, 10);

        Assert.True(area.IsValidForLevel(5));
    }

    [Fact]
    public void IsValidForLevel_BelowRange_ReturnsFalse()
    {
        var area = new Area("starter_city", "Starter City", "desc", 5, 15);

        Assert.False(area.IsValidForLevel(3));
    }

    [Fact]
    public void IsValidForLevel_AboveRange_ReturnsFalse()
    {
        var area = new Area("starter_city", "Starter City", "desc", 1, 10);

        Assert.False(area.IsValidForLevel(20));
    }

    [Fact]
    public void IsValidForLevel_AtMinimum_ReturnsTrue()
    {
        var area = new Area("starter_city", "Starter City", "desc", 5, 15);

        Assert.True(area.IsValidForLevel(5));
    }

    [Fact]
    public void IsValidForLevel_AtMaximum_ReturnsTrue()
    {
        var area = new Area("starter_city", "Starter City", "desc", 5, 15);

        Assert.True(area.IsValidForLevel(15));
    }

    [Fact]
    public void GetLevelRangeString_ReturnsFormatted()
    {
        var area = new Area("starter_city", "Starter City", "desc", 1, 10);

        Assert.Equal("1-10", area.GetLevelRangeString());
    }
}
