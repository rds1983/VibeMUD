namespace VibeMUD.Systems;

using VibeMUD.Models;

/// <summary>
/// Manages turn-based combat between characters and NPCs
/// </summary>
public class CombatSystem
{
    private const int BaseDamageVariance = 3; // +/- damage variance

    public class CombatResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DamageDealt { get; set; }
        public bool TargetDefeated { get; set; }
        public bool AttackerDefeated { get; set; }
        public long ExperienceAwarded { get; set; }
    }

    /// <summary>
    /// Calculates damage based on attacker stats and defender armor
    /// </summary>
    public static int CalculateDamage(int baseAttack, int attackerStat, int defenderArmor, string damageType = "physical")
    {
        var random = new Random();

        // Base damage with variance
        int variance = random.Next(-BaseDamageVariance, BaseDamageVariance + 1);
        int baseDamage = Math.Max(1, baseAttack + variance);

        // Apply stat modifier (every 2 points = +1 damage)
        int statBonus = attackerStat / 2;
        int totalDamage = baseDamage + statBonus;

        // Reduce by armor (every armor point = -1 damage, minimum 0)
        totalDamage = Math.Max(1, totalDamage - defenderArmor);

        return totalDamage;
    }

    /// <summary>
    /// Applies resistance/vulnerability modifier to damage
    /// </summary>
    public static int ApplyResistance(int damage, string damageType, Dictionary<string, double> resistances)
    {
        if (!resistances.TryGetValue(damageType, out var resistance))
            return damage;

        double multiplier = 1.0 + resistance;
        return Math.Max(1, (int)(damage * multiplier));
    }

    /// <summary>
    /// Applies vulnerability modifiers (special flags, damage types)
    /// </summary>
    public static int ApplyVulnerabilities(int damage, string damageType, NPC npc)
    {
        double multiplier = 1.0;

        // Light damage vs Undead
        if (damageType == "light" && npc.HasFlag("undead"))
            multiplier *= 1.5;

        // Physical damage vs Ethereal
        if (damageType == "physical" && npc.HasFlag("ethereal"))
            multiplier *= 0.5;

        return Math.Max(1, (int)(damage * multiplier));
    }

    /// <summary>
    /// Calculates initiative (who goes first)
    /// </summary>
    public static int CalculateInitiative(int dexterity)
    {
        var random = new Random();
        return dexterity + random.Next(1, 21); // d20 + dex
    }

    /// <summary>
    /// Character attacks NPC
    /// </summary>
    public CombatResult CharacterAttacksNPC(Character character, NPC npc, string weaponType = "physical")
    {
        var result = new CombatResult();

        // Calculate base damage (from strength stat and weapon)
        int baseAttack = 5 + (character.GetEffectiveStat("strength") / 3);
        int damage = CalculateDamage(baseAttack, character.GetEffectiveStat("strength"), 0);

        // Apply resistances
        damage = ApplyResistance(damage, weaponType, npc.Resistances);

        // Apply vulnerabilities
        damage = ApplyVulnerabilities(damage, weaponType, npc);

        // Apply damage
        bool npcDefeated = npc.TakeDamage(damage, weaponType);

        result.Success = true;
        result.DamageDealt = damage;
        result.TargetDefeated = npcDefeated;
        result.Message = $"You attack the {npc.Name} for {damage} damage!";

        if (npcDefeated)
        {
            result.Message += $" The {npc.Name} has been defeated!";
            long xpReward = ProgressionSystem.CalculateExperienceReward(character, npc);
            ProgressionSystem.AwardExperience(character, xpReward);
            result.ExperienceAwarded = xpReward;
            result.Message += $" You gain {xpReward} experience!";
        }

        return result;
    }

    /// <summary>
    /// NPC attacks Character
    /// </summary>
    public CombatResult NPCAttacksCharacter(NPC npc, Character character)
    {
        var result = new CombatResult();

        // Calculate base damage
        int baseAttack = 3 + (npc.Strength / 3);
        int damage = CalculateDamage(baseAttack, npc.Strength, character.GetTotalArmor());

        // Apply character armor reduction (each armor point = -1 damage)
        damage = Math.Max(1, damage - (character.GetTotalArmor() / 2));

        // Apply damage
        bool characterDefeated = character.TakeDamage(damage);

        result.Success = true;
        result.DamageDealt = damage;
        result.AttackerDefeated = characterDefeated;
        result.Message = $"The {npc.Name} attacks you for {damage} damage!";

        if (characterDefeated)
        {
            result.Message += $" You have been defeated!";
        }

        return result;
    }

    /// <summary>
    /// Character uses a skill
    /// </summary>
    public CombatResult UseSkill(Character character, NPC npc, Skill skill)
    {
        var result = new CombatResult();

        // Check mana
        if (character.Mana < skill.ManaCost)
        {
            result.Success = false;
            result.Message = $"You don't have enough mana! (Need: {skill.ManaCost}, Have: {character.Mana})";
            return result;
        }

        // Consume mana
        character.Mana -= skill.ManaCost;

        // Calculate damage with intelligence bonus
        int baseDamage = skill.Damage + (character.GetEffectiveStat("intelligence") / 4);
        int damage = CalculateDamage(baseDamage, character.GetEffectiveStat("intelligence"), 0, skill.DamageType);

        // Apply resistances
        damage = ApplyResistance(damage, skill.DamageType, npc.Resistances);

        // Apply vulnerabilities
        damage = ApplyVulnerabilities(damage, skill.DamageType, npc);

        // Apply damage
        bool npcDefeated = npc.TakeDamage(damage, skill.DamageType);

        result.Success = true;
        result.DamageDealt = damage;
        result.TargetDefeated = npcDefeated;
        result.Message = $"You cast {skill.Name} for {damage} {skill.DamageType} damage!";

        if (npcDefeated)
        {
            result.Message += $" The {npc.Name} has been defeated!";
            long xpReward = ProgressionSystem.CalculateExperienceReward(character, npc);
            ProgressionSystem.AwardExperience(character, xpReward);
            result.ExperienceAwarded = xpReward;
            result.Message += $" You gain {xpReward} experience!";
        }

        return result;
    }

    /// <summary>
    /// Character uses a potion
    /// </summary>
    public CombatResult UsePotion(Character character, Item potion)
    {
        var result = new CombatResult();

        if (!potion.Stats.TryGetValue("heal", out var healAmount))
        {
            result.Success = false;
            result.Message = "This potion cannot be used.";
            return result;
        }

        character.Heal(healAmount);
        result.Success = true;
        result.Message = $"You drink the {potion.Name} and recover {healAmount} health!";

        return result;
    }

    /// <summary>
    /// Character attempts to flee combat
    /// </summary>
    public CombatResult AttemptFlee(Character character, NPC npc)
    {
        var result = new CombatResult();
        var random = new Random();

        // Flee chance: 50% base + (character DEX - NPC DEX) * 2%
        int fleeChance = 50 + ((character.GetEffectiveStat("dexterity") - npc.Dexterity) * 2);
        fleeChance = Math.Max(20, Math.Min(80, fleeChance)); // Clamp between 20-80%

        bool escaped = random.Next(100) < fleeChance;

        result.Success = escaped;

        if (escaped)
        {
            result.Message = $"You successfully flee from the {npc.Name}!";
        }
        else
        {
            result.Message = $"You failed to escape from the {npc.Name}!";

            // NPC gets a free attack if flee fails
            var npcAttack = NPCAttacksCharacter(npc, character);
            result.Message += $" {npcAttack.Message}";
            result.AttackerDefeated = npcAttack.AttackerDefeated;
        }

        return result;
    }

    /// <summary>
    /// NPC uses special attack
    /// </summary>
    public CombatResult NPCSpecialAttack(NPC npc, Character character, string specialAttackId)
    {
        var result = new CombatResult();

        // Special attack definitions
        var specialAttacks = new Dictionary<string, (int damage, string type, string message)>
        {
            ["bite_attack"] = (8, "physical", "bites you"),
            ["poison_bite"] = (6, "physical", "injects poison into you"), // Poison damage over time
            ["fire_breath"] = (15, "fire", "breathes fire"),
            ["frost_bolt"] = (12, "ice", "casts frost bolt"),
            ["lightning_strike"] = (10, "lightning", "strikes you with lightning"),
            ["petrify_gaze"] = (0, "light", "gazes at you with petrifying eyes"),
            ["death_roll"] = (10, "physical", "performs a death roll")
        };

        if (!specialAttacks.TryGetValue(specialAttackId, out var attack))
        {
            return NPCAttacksCharacter(npc, character);
        }

        int damage = Math.Max(1, attack.damage + npc.Strength / 4);
        bool characterDefeated = character.TakeDamage(damage);

        result.Success = true;
        result.DamageDealt = damage;
        result.AttackerDefeated = characterDefeated;
        result.Message = $"The {npc.Name} {attack.message} for {damage} damage!";

        if (characterDefeated)
        {
            result.Message += " You have been defeated!";
        }

        return result;
    }
}
