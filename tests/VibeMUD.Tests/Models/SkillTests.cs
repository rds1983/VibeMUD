namespace VibeMUD.Tests.Models;

using VibeMUD.Models;

public class SkillTests
{
    [Fact]
    public void Constructor_WithAllParameters_InitializesCorrectly()
    {
        var skill = new Skill("fireball", "Fireball", "Fire spell", 30, 5, 40, "fire", true);

        Assert.Equal("fireball", skill.Id);
        Assert.Equal("Fireball", skill.Name);
        Assert.Equal("Fire spell", skill.Description);
        Assert.Equal(30, skill.ManaCost);
        Assert.Equal(5, skill.CooldownSeconds);
        Assert.Equal(40, skill.Damage);
        Assert.Equal("fire", skill.DamageType);
        Assert.True(skill.IsOffensive);
    }

    [Fact]
    public void CanUse_WithSufficientManaAndNoCooldown_ReturnsTrue()
    {
        var skill = new Skill("fireball", "Fireball", "Fire spell", 30, 5, 40, "fire", true);
        var lastUsed = DateTime.UtcNow.AddSeconds(-10);

        Assert.True(skill.CanUse(50, lastUsed));
    }

    [Fact]
    public void CanUse_WithInsufficientMana_ReturnsFalse()
    {
        var skill = new Skill("fireball", "Fireball", "Fire spell", 30, 5, 40, "fire", true);
        var lastUsed = DateTime.UtcNow.AddSeconds(-10);

        Assert.False(skill.CanUse(20, lastUsed));
    }

    [Fact]
    public void CanUse_WithActiveCooldown_ReturnsFalse()
    {
        var skill = new Skill("fireball", "Fireball", "Fire spell", 30, 5, 40, "fire", true);
        var lastUsed = DateTime.UtcNow.AddSeconds(-2);

        Assert.False(skill.CanUse(50, lastUsed));
    }

    [Fact]
    public void CanUse_WithExactCooldownExpired_ReturnsTrue()
    {
        var skill = new Skill("fireball", "Fireball", "Fire spell", 30, 5, 40, "fire", true);
        var lastUsed = DateTime.UtcNow.AddSeconds(-5);

        Assert.True(skill.CanUse(50, lastUsed));
    }
}
