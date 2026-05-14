namespace VibeMUD.Tests.Data;

using VibeMUD.Data;
using VibeMUD.Models;

public class JsonDataLoaderTests : IDisposable
{
    private readonly string _testContentPath;
    private readonly JsonDataLoader _loader;

    public JsonDataLoaderTests()
    {
        _testContentPath = Path.Combine(Path.GetTempPath(), "vibemund_content_" + Guid.NewGuid());
        Directory.CreateDirectory(_testContentPath);
        _loader = new JsonDataLoader(_testContentPath);
    }

    [Fact]
    public void LoadSkills_ValidJson_ReturnsSkills()
    {
        var skillsJson = @"{
            ""skills"": [
                {
                    ""id"": ""fireball"",
                    ""name"": ""Fireball"",
                    ""description"": ""Fire spell"",
                    ""manaCost"": 30,
                    ""cooldownSeconds"": 5,
                    ""damage"": 40,
                    ""damageType"": ""fire"",
                    ""isOffensive"": true,
                    ""isHealing"": false,
                    ""healAmount"": 0,
                    ""requiredLevel"": 5
                }
            ]
        }";

        File.WriteAllText(Path.Combine(_testContentPath, "skills.json"), skillsJson);
        var skills = _loader.LoadSkills();

        Assert.NotEmpty(skills);
        Assert.True(skills.ContainsKey("fireball"));
        Assert.Equal("Fireball", skills["fireball"].Name);
    }

    [Fact]
    public void LoadSkills_MissingFile_ReturnsEmpty()
    {
        var skills = _loader.LoadSkills();

        Assert.Empty(skills);
    }

    [Fact]
    public void LoadItems_ValidJson_ReturnsItems()
    {
        var itemsJson = @"{
            ""items"": [
                {
                    ""id"": ""iron_sword"",
                    ""name"": ""Iron Sword"",
                    ""type"": ""weapon"",
                    ""subtype"": ""sword"",
                    ""slot"": ""main-hand"",
                    ""weight"": 3.5,
                    ""value"": 50,
                    ""requiredLevel"": 1,
                    ""stats"": { ""damage"": 8 },
                    ""resistances"": {},
                    ""damageType"": ""physical""
                }
            ]
        }";

        File.WriteAllText(Path.Combine(_testContentPath, "items.json"), itemsJson);
        var items = _loader.LoadItems();

        Assert.NotEmpty(items);
        Assert.True(items.ContainsKey("iron_sword"));
        Assert.Equal("Iron Sword", items["iron_sword"].Name);
    }

    [Fact]
    public void LoadAreas_ValidJson_ReturnsAreas()
    {
        var areasJson = @"{
            ""areas"": [
                {
                    ""id"": ""starter_city"",
                    ""name"": ""Starter City"",
                    ""description"": ""A welcoming city"",
                    ""levelRange"": { ""min"": 1, ""max"": 10 },
                    ""spawnPoint"": { ""areaId"": ""starter_city"", ""roomId"": ""city_center"" },
                    ""hasShop"": true,
                    ""shopId"": ""starter_shop"",
                    ""rooms"": [
                        {
                            ""id"": ""city_center"",
                            ""title"": ""City Center"",
                            ""description"": ""The heart of the city"",
                            ""terrain"": ""city"",
                            ""exits"": {
                                ""north"": null,
                                ""south"": null,
                                ""east"": null,
                                ""west"": null,
                                ""up"": null,
                                ""down"": null
                            },
                            ""npcs"": [""guard_captain""],
                            ""items"": []
                        }
                    ]
                }
            ]
        }";

        File.WriteAllText(Path.Combine(_testContentPath, "areas.json"), areasJson);
        var areas = _loader.LoadAreas();

        Assert.NotEmpty(areas);
        Assert.True(areas.ContainsKey("starter_city"));
        Assert.Equal("Starter City", areas["starter_city"].Name);
        Assert.Equal(1, areas["starter_city"].MinLevel);
        Assert.Equal(10, areas["starter_city"].MaxLevel);
    }

    [Fact]
    public void LoadAreas_WithRooms_LoadsRoomsCorrectly()
    {
        var areasJson = @"{
            ""areas"": [
                {
                    ""id"": ""starter_city"",
                    ""name"": ""Starter City"",
                    ""description"": ""A welcoming city"",
                    ""levelRange"": { ""min"": 1, ""max"": 10 },
                    ""spawnPoint"": null,
                    ""hasShop"": false,
                    ""shopId"": null,
                    ""rooms"": [
                        {
                            ""id"": ""city_center"",
                            ""title"": ""City Center"",
                            ""description"": ""The heart of the city"",
                            ""terrain"": ""city"",
                            ""exits"": {
                                ""north"": { ""areaId"": ""starter_city"", ""roomId"": ""temple"" },
                                ""south"": null,
                                ""east"": null,
                                ""west"": null,
                                ""up"": null,
                                ""down"": null
                            },
                            ""npcs"": [""guard""],
                            ""items"": []
                        }
                    ]
                }
            ]
        }";

        File.WriteAllText(Path.Combine(_testContentPath, "areas.json"), areasJson);
        var areas = _loader.LoadAreas();

        Assert.True(areas["starter_city"].RoomExists("city_center"));
        var room = areas["starter_city"].GetRoom("city_center");
        Assert.NotNull(room);
        Assert.True(room!.HasExit("north"));
        Assert.Contains("guard", room.NPCIds);
    }

    [Fact]
    public void LoadNPCs_ValidJson_ReturnsNPCs()
    {
        var npcsJson = @"{
            ""npcs"": [
                {
                    ""id"": ""goblin_warrior"",
                    ""name"": ""Goblin Warrior"",
                    ""description"": ""A scrappy goblin"",
                    ""level"": 3,
                    ""health"": 20,
                    ""maxHealth"": 20,
                    ""mana"": 5,
                    ""maxMana"": 5,
                    ""strength"": 12,
                    ""dexterity"": 11,
                    ""constitution"": 10,
                    ""intelligence"": 8,
                    ""wisdom"": 9,
                    ""charisma"": 7,
                    ""behavior"": ""aggressive"",
                    ""patrolPath"": [],
                    ""skillIds"": [],
                    ""specialAttackIds"": [],
                    ""flags"": [],
                    ""goldMin"": 5,
                    ""goldMax"": 15,
                    ""lootTable"": {},
                    ""resistances"": { ""physical"": 0 },
                    ""damageTypes"": [""physical""],
                    ""respawnTimeSeconds"": 300
                }
            ]
        }";

        File.WriteAllText(Path.Combine(_testContentPath, "npcs.json"), npcsJson);
        var npcs = _loader.LoadNPCs();

        Assert.NotEmpty(npcs);
        Assert.True(npcs.ContainsKey("goblin_warrior"));
        Assert.Equal("Goblin Warrior", npcs["goblin_warrior"].Name);
    }

    [Fact]
    public void ValidateJsonFile_ValidFile_ReturnsTrue()
    {
        var json = @"{ ""test"": ""data"" }";
        File.WriteAllText(Path.Combine(_testContentPath, "test.json"), json);

        Assert.True(_loader.ValidateJsonFile("test.json"));
    }

    [Fact]
    public void ValidateJsonFile_InvalidJson_ReturnsFalse()
    {
        var json = @"{ invalid json }";
        File.WriteAllText(Path.Combine(_testContentPath, "test.json"), json);

        Assert.False(_loader.ValidateJsonFile("test.json"));
    }

    [Fact]
    public void ValidateJsonFile_MissingFile_ReturnsFalse()
    {
        Assert.False(_loader.ValidateJsonFile("nonexistent.json"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testContentPath))
            Directory.Delete(_testContentPath, true);
    }
}
