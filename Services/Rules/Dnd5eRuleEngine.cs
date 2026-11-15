using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Models.Equipment;
using DnDCharacterManager.Services.Rules.Dice;
using Enemy = DnDCharacterManager.Models.Enemies.Enemy;
using EnemyAttack = DnDCharacterManager.Models.Enemies.EnemyAttack;

namespace DnDCharacterManager.Services.Rules;

public interface IRuleEngine
{
    int CalculateProficiencyBonus(int level);
    int CalculateAbilityModifier(int score);
    int CalculateSkillModifier(Character character, string skillName);
    int CalculateSavingThrowModifier(Character character, AbilityType ability);
    int CalculateArmorClass(Character character);
    int CalculateInitiativeModifier(Character character);
    int CalculateAttackBonus(Character character, Weapon weapon, bool isRanged);
    (string DamageExpression, string DamageType) CalculateDamage(Character character, Weapon weapon);
    int CalculateMaxHitPoints(Character character, int classHitDie, int constitutionModifier);
    int CalculateEnemyProficiencyBonus(string challengeRating);
    int CalculateEnemyAbilityModifier(int score);
    int CalculateEnemyAttackBonus(Enemy enemy, EnemyAttack attack);
    (string DamageExpression, string DamageType) CalculateEnemyDamage(EnemyAttack attack);
}

/// <summary>Centralized calculation service. Edition-specific behavior should be selected by ruleset.</summary>
public sealed class Dnd5eRuleEngine : IRuleEngine
{
    private static readonly IReadOnlyDictionary<string, AbilityType> SkillAbilityMap =
        new Dictionary<string, AbilityType>(StringComparer.OrdinalIgnoreCase)
        {
            ["Acrobatics"] = AbilityType.Dexterity,
            ["Animal Handling"] = AbilityType.Wisdom,
            ["Arcana"] = AbilityType.Intelligence,
            ["Athletics"] = AbilityType.Strength,
            ["Deception"] = AbilityType.Charisma,
            ["History"] = AbilityType.Intelligence,
            ["Insight"] = AbilityType.Wisdom,
            ["Intimidation"] = AbilityType.Charisma,
            ["Investigation"] = AbilityType.Intelligence,
            ["Medicine"] = AbilityType.Wisdom,
            ["Nature"] = AbilityType.Intelligence,
            ["Perception"] = AbilityType.Wisdom,
            ["Performance"] = AbilityType.Charisma,
            ["Persuasion"] = AbilityType.Charisma,
            ["Religion"] = AbilityType.Intelligence,
            ["Sleight of Hand"] = AbilityType.Dexterity,
            ["Stealth"] = AbilityType.Dexterity,
            ["Survival"] = AbilityType.Wisdom
        };

    public int CalculateProficiencyBonus(int level)
    {
        var safeLevel = Math.Clamp(level, 1, 20);
        return 2 + (safeLevel - 1) / 4;
    }

    public int CalculateAbilityModifier(int score) => Character.CalculateModifier(score);

    public int CalculateSkillModifier(Character character, string skillName)
    {
        var skill = character.Skills.FirstOrDefault(s =>
            s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

        if (skill is null)
        {
            return SkillAbilityMap.TryGetValue(skillName, out var ability)
                ? CalculateAbilityModifier(character.GetAbilityScore(ability))
                : 0;
        }

        var abilityModifier = CalculateAbilityModifier(character.GetAbilityScore(skill.Ability));
        var proficiencyBonus = skill.IsProficient ? CalculateProficiencyBonus(character.Level) : 0;
        var expertiseBonus = skill.HasExpertise ? CalculateProficiencyBonus(character.Level) : 0;
        return abilityModifier + proficiencyBonus + expertiseBonus + skill.MiscModifier;
    }

    public int CalculateSavingThrowModifier(Character character, AbilityType ability)
    {
        // Saving-throw proficiency must be added from a character's selected class/ruleset data.
        return CalculateAbilityModifier(character.GetAbilityScore(ability));
    }

    public int CalculateArmorClass(Character character)
    {
        var equippedArmor = character.Armor.FirstOrDefault(a =>
            a.IsEquipped && a.Armor is not null &&
            !a.Armor.Category.Equals("Shield", StringComparison.OrdinalIgnoreCase));

        var baseAc = equippedArmor?.Armor?.BaseAC ?? 10;
        var maxDexBonus = equippedArmor?.Armor?.MaxDexBonus ?? int.MaxValue;
        var dexterityBonus = Math.Min(character.DexterityMod, maxDexBonus);

        var hasShield = character.Armor.Any(a => a.IsEquipped &&
                             a.Armor?.Category.Equals("Shield", StringComparison.OrdinalIgnoreCase) == true)
                        || character.Equipment.Any(e => e.IsEquipped &&
                             e.Item?.Category.Equals("Shield", StringComparison.OrdinalIgnoreCase) == true);

        return baseAc + dexterityBonus + (hasShield ? 2 : 0);
    }

    public int CalculateInitiativeModifier(Character character) => character.DexterityMod;

    public int CalculateAttackBonus(Character character, Weapon weapon, bool isRanged)
    {
        var finesse = weapon.Properties.Contains("Finesse", StringComparison.OrdinalIgnoreCase);
        var abilityModifier = isRanged
            ? character.DexterityMod
            : finesse ? Math.Max(character.StrengthMod, character.DexterityMod) : character.StrengthMod;

        var ownedWeapon = character.Weapons.FirstOrDefault(w => w.WeaponId == weapon.Id);
        var proficiencyBonus = ownedWeapon?.IsProficient == true
            ? CalculateProficiencyBonus(character.Level)
            : 0;

        return abilityModifier + proficiencyBonus;
    }

    public (string DamageExpression, string DamageType) CalculateDamage(Character character, Weapon weapon)
    {
        var finesse = weapon.Properties.Contains("Finesse", StringComparison.OrdinalIgnoreCase);
        var abilityModifier = finesse
            ? Math.Max(character.StrengthMod, character.DexterityMod)
            : character.StrengthMod;
        var sign = abilityModifier >= 0 ? "+" : string.Empty;
        return ($"{weapon.DamageDice}{sign}{abilityModifier}", weapon.DamageType);
    }

    public int CalculateMaxHitPoints(Character character, int classHitDie, int constitutionModifier)
    {
        if (classHitDie is < 1 or > 20)
            throw new ArgumentOutOfRangeException(nameof(classHitDie));

        var level = Math.Clamp(character.Level, 1, 20);
        if (level == 1)
            return Math.Max(1, classHitDie + constitutionModifier);

        var averagePerLevel = (classHitDie / 2) + 1 + constitutionModifier;
        return Math.Max(1, classHitDie + constitutionModifier + averagePerLevel * (level - 1));
    }

    public int CalculateEnemyProficiencyBonus(string challengeRating) => challengeRating switch
    {
        "0" or "1/8" or "1/4" or "1/2" or "1" or "2" or "3" or "4" => 2,
        "5" or "6" or "7" or "8" => 3,
        "9" or "10" or "11" or "12" => 4,
        "13" or "14" or "15" or "16" => 5,
        "17" or "18" or "19" or "20" => 6,
        "21" or "22" or "23" or "24" => 7,
        "25" or "26" or "27" or "28" => 8,
        "29" or "30" => 9,
        _ => 2
    };

    public int CalculateEnemyAbilityModifier(int score) => Character.CalculateModifier(score);

    public int CalculateEnemyAttackBonus(Enemy enemy, EnemyAttack attack) => attack.AttackBonus;

    public (string DamageExpression, string DamageType) CalculateEnemyDamage(EnemyAttack attack)
        => (attack.DamageDice, attack.DamageType);
}
