namespace VibeMUD.Tests.Data;

using VibeMUD.Data;
using VibeMUD.Models;

public class SaveManagerTests : IDisposable
{
    private readonly string _testSavePath;
    private readonly SaveManager _saveManager;

    public SaveManagerTests()
    {
        _testSavePath = Path.Combine(Path.GetTempPath(), "vibemund_test_" + Guid.NewGuid());
        Directory.CreateDirectory(_testSavePath);
        _saveManager = new SaveManager(_testSavePath);
    }

    [Fact]
    public void SaveCharacter_CreatesFile()
    {
        var character = new Character("test_player", "TestChar", "warrior");

        _saveManager.SaveCharacter(character);

        Assert.True(File.Exists(Path.Combine(_testSavePath, "test_player.json")));
    }

    [Fact]
    public void SaveCharacter_WithEmptyId_ThrowsException()
    {
        var character = new Character("", "TestChar", "warrior");

        Assert.Throws<ArgumentException>(() => _saveManager.SaveCharacter(character));
    }

    [Fact]
    public void LoadCharacter_ExistingCharacter_ReturnsCharacter()
    {
        var original = new Character("test_player", "TestChar", "warrior") { Level = 5, Gold = 100 };
        _saveManager.SaveCharacter(original);

        var loaded = _saveManager.LoadCharacter("test_player");

        Assert.NotNull(loaded);
        Assert.Equal("TestChar", loaded?.Name);
        Assert.Equal(5, loaded?.Level);
        Assert.Equal(100, loaded?.Gold);
    }

    [Fact]
    public void LoadCharacter_NonExistingCharacter_ReturnsNull()
    {
        var loaded = _saveManager.LoadCharacter("nonexistent");

        Assert.Null(loaded);
    }

    [Fact]
    public void CharacterExists_ExistingCharacter_ReturnsTrue()
    {
        var character = new Character("test_player", "TestChar", "warrior");
        _saveManager.SaveCharacter(character);

        Assert.True(_saveManager.CharacterExists("test_player"));
    }

    [Fact]
    public void CharacterExists_NonExistingCharacter_ReturnsFalse()
    {
        Assert.False(_saveManager.CharacterExists("nonexistent"));
    }

    [Fact]
    public void GetAllCharacterIds_ReturnsAllIds()
    {
        _saveManager.SaveCharacter(new Character("player1", "Player One", "warrior"));
        _saveManager.SaveCharacter(new Character("player2", "Player Two", "mage"));
        _saveManager.SaveCharacter(new Character("player3", "Player Three", "thief"));

        var ids = _saveManager.GetAllCharacterIds();

        Assert.Contains("player1", ids);
        Assert.Contains("player2", ids);
        Assert.Contains("player3", ids);
        Assert.Equal(3, ids.Count);
    }

    [Fact]
    public void DeleteCharacter_ExistingCharacter_Deleted()
    {
        var character = new Character("test_player", "TestChar", "warrior");
        _saveManager.SaveCharacter(character);
        Assert.True(_saveManager.CharacterExists("test_player"));

        _saveManager.DeleteCharacter("test_player");

        Assert.False(_saveManager.CharacterExists("test_player"));
    }

    [Fact]
    public void DeleteCharacter_NonExistingCharacter_DoesNotThrow()
    {
        _saveManager.DeleteCharacter("nonexistent");
        // Should not throw
    }

    public void Dispose()
    {
        if (Directory.Exists(_testSavePath))
            Directory.Delete(_testSavePath, true);
    }
}
