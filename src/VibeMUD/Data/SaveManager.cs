namespace VibeMUD.Data;

using System.Text.Json;
using VibeMUD.Models;

/// <summary>
/// Manages saving and loading character data
/// </summary>
public class SaveManager
{
    private readonly string _savePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public SaveManager(string savePath)
    {
        _savePath = savePath;
        Directory.CreateDirectory(_savePath);
    }

    public void SaveCharacter(Character character)
    {
        if (string.IsNullOrEmpty(character.Name))
            throw new ArgumentException("Character must have a Name to save");

        try
        {
            var safeFileName = SanitizeFileName(character.Name);
            var filePath = Path.Combine(_savePath, $"{safeFileName}.json");
            var json = JsonSerializer.Serialize(character, JsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save character {character.Name}", ex);
        }
    }

    public Character? LoadCharacter(string characterId)
    {
        try
        {
            var filePath = Path.Combine(_savePath, $"{characterId}.json");

            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Character>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load character {characterId}", ex);
        }
    }

    public Character? LoadCharacterByName(string characterName)
    {
        try
        {
            if (string.IsNullOrEmpty(characterName))
                return null;

            var safeFileName = SanitizeFileName(characterName);
            var filePath = Path.Combine(_savePath, $"{safeFileName}.json");

            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Character>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load character {characterName}", ex);
        }
    }

    public bool CharacterExists(string characterId)
    {
        var filePath = Path.Combine(_savePath, $"{characterId}.json");
        return File.Exists(filePath);
    }

    public bool CharacterExistsByName(string characterName)
    {
        try
        {
            if (string.IsNullOrEmpty(characterName))
                return false;

            var safeFileName = SanitizeFileName(characterName);
            var filePath = Path.Combine(_savePath, $"{safeFileName}.json");
            return File.Exists(filePath);
        }
        catch
        {
            return false;
        }
    }

    public List<string> GetAllCharacterIds()
    {
        try
        {
            if (!Directory.Exists(_savePath))
                return new List<string>();

            var characterIds = new List<string>();
            var files = Directory.GetFiles(_savePath, "*.json");

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                characterIds.Add(fileName);
            }

            return characterIds;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get character list", ex);
        }
    }

    public void DeleteCharacter(string characterId)
    {
        try
        {
            var filePath = Path.Combine(_savePath, $"{characterId}.json");

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete character {characterId}", ex);
        }
    }

    private static string SanitizeFileName(string name)
    {
        return name.ToLower().Replace(" ", "_").Trim();
    }
}
