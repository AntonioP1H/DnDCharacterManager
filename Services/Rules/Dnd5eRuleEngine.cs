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
        int safeLevel = Math.Clamp(level, 1, 20);
        return 2 + (safeLevel - 1) / 4;
    }

    public int CalculateAbilityModifier(int score) => Character.CalculateModifier(score);

    public int CalculateSkillModifier(Character character, string skillName)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentException.ThrowIfNullOrWhiteSpace(skillName);

        Skill? skill = character.Skills.FirstOrDefault(s =>
            s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

        if (skill is null)
            return SkillAbilityMap.TryGetValue(skillName, out AbilityType ability)
                ? character.GetAbilityModifier(ability)
                : 0;

        int proficiency = skill.HasExpertise
            ? CalculateProficiencyBonus(character.Level) * 2
            : skill.IsProficient ? CalculateProficiencyBonus(character.Level) : 0;

        return character.GetAbilityModifier(skill.Ability) + proficiency + skill.MiscModifier;
    }

    public int CalculateSavingThrowModifier(Character character, AbilityType ability)
    {
        ArgumentNullException.ThrowIfNull(character);
        // The current Character model does not yet persist saving-throw proficiencies.
        return character.GetAbilityModifier(ability);
    }

    public int CalculateArmorClass(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);

        var equippedArmor = character.Armor
            .Where(x => x.IsEquipped && x.Armor is not null)
            .Select(x => x.Armor!)
            .FirstOrDefault(a => !a.Category.Equals("Shield", StringComparison.OrdinalIgnoreCase));

        bool hasShield = character.Armor.Any(x => x.IsEquipped &&
            x.Armor?.Category.Equals("Shield", StringComparison.OrdinalIgnoreCase) == true);

        int dexterityModifier = character.DexterityMod;
        int baseArmorClass = equippedArmor?.BaseAC ?? 10;
        int dexterityContribution;

        if (equippedArmor is null)
        {
            dexterityContribution = dexterityModifier;
        }
        else if (equippedArmor.Category.Equals("Heavy", StringComparison.OrdinalIgnoreCase))
        {
            dexterityContribution = 0;
        }
        else if (equippedArmor.MaxDexBonus.HasValue)
        {
            dexterityContribution = Math.Min(dexterityModifier, equippedArmor.MaxDexBonus.Value);
        }
        else
        {
            dexterityContribution = dexterityModifier;
        }

        int shieldBonus = hasShield ? 2 : 0;
        return baseArmorClass + dexterityContribution + shieldBonus;
    }

    public int CalculateInitiativeModifier(Character character)
    {
        ArgumentNullException.ThrowIfNull(character);
        return character.DexterityMod;
    }

    public int CalculateAttackBonus(Character character, Weapon weapon, bool isRanged)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(weapon);

        bool finesse = HasProperty(weapon, "Finesse");
        bool thrown = HasProperty(weapon, "Thrown");
        int abilityModifier;

        if (finesse)
            abilityModifier = Math.Max(character.StrengthMod, character.DexterityMod);
        else if (isRanged && !thrown)
            abilityModifier = character.DexterityMod;
        else
            abilityModifier = character.StrengthMod;

        bool proficient = character.Weapons.Any(w => w.WeaponId == weapon.Id && w.IsProficient);
        int proficiencyBonus = proficient ? character.ProficiencyBonus : 0;
        return abilityModifier + proficiencyBonus;
    }

    public (string DamageExpression, string DamageType) CalculateDamage(Character character, Weapon weapon)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(weapon);

        bool finesse = HasProperty(weapon, "Finesse");
        bool thrown = HasProperty(weapon, "Thrown");
        int abilityModifier = finesse
            ? Math.Max(character.StrengthMod, character.DexterityMod)
            : weapon.IsRanged && !thrown ? character.DexterityMod : character.StrengthMod;

        string expression = abilityModifier == 0
            ? weapon.DamageDice
            : $"{weapon.DamageDice}{(abilityModifier > 0 ? "+" : string.Empty)}{abilityModifier}";

        return (expression, weapon.DamageType);
    }

    private static bool HasProperty(Weapon weapon, string property)
        => weapon.PropertyList.Any(p => p.Equals(property, StringComparison.OrdinalIgnoreCase) ||
                                        p.StartsWith(property + " ", StringComparison.OrdinalIgnoreCase));

    public int CalculateMaxHitPoints(Character character, int classHitDie, int constitutionModifier)
    {
        ArgumentNullException.ThrowIfNull(character);
        if (classHitDie is not (6 or 8 or 10 or 12))
            throw new ArgumentOutOfRangeException(nameof(classHitDie), "Class hit die must be d6, d8, d10, or d12.");

        int level = Math.Clamp(character.Level, 1, 20);
        int averageAfterFirst = (classHitDie / 2) + 1 + constitutionModifier;
        return classHitDie + constitutionModifier + averageAfterFirst * (level - 1);
    }

    public int CalculateEnemyProficiencyBonus(string challengeRating)
        => challengeRating?.Trim() switch
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

    public int CalculateEnemyAttackBonus(Enemy enemy, EnemyAttack attack)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        ArgumentNullException.ThrowIfNull(attack);
        return attack.AttackBonus;
    }

    public (string DamageExpression, string DamageType) CalculateEnemyDamage(EnemyAttack attack)
    {
        ArgumentNullException.ThrowIfNull(attack);
        return (attack.DamageDice, attack.DamageType);
    }
}
