using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using DnDCharacterManager.Models.Equipment;

namespace DnDCharacterManager.Models.Characters;

/// <summary>Player character entity.</summary>
[Table("Characters")]
public partial class Character : ObservableObject
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string PlayerName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Class { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Race { get; set; } = string.Empty;

    public int Level { get; set; } = 1;

    [MaxLength(100)]
    public string Background { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Alignment { get; set; } = "Neutral";

    [MaxLength(500)]
    public string? PortraitPath { get; set; }

    public int CurrentHitPoints { get; set; }
    public int TemporaryHitPoints { get; set; }
    public int MaxHitPoints { get; set; }
    public int ArmorClass { get; set; }
    public int InitiativeModifier { get; set; }
    public int Speed { get; set; } = 30;

    [NotMapped]
    public int ProficiencyBonus => CalculateProficiencyBonus();

    public virtual ICollection<AbilityScore> AbilityScores { get; set; } = new List<AbilityScore>();
    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public virtual ICollection<CharacterEquipment> Equipment { get; set; } = new List<CharacterEquipment>();
    public virtual ICollection<CharacterWeapon> Weapons { get; set; } = new List<CharacterWeapon>();
    public virtual ICollection<CharacterArmor> Armor { get; set; } = new List<CharacterArmor>();

    [NotMapped] public int Strength => GetAbilityScore(AbilityType.Strength);
    [NotMapped] public int Dexterity => GetAbilityScore(AbilityType.Dexterity);
    [NotMapped] public int Constitution => GetAbilityScore(AbilityType.Constitution);
    [NotMapped] public int Intelligence => GetAbilityScore(AbilityType.Intelligence);
    [NotMapped] public int Wisdom => GetAbilityScore(AbilityType.Wisdom);
    [NotMapped] public int Charisma => GetAbilityScore(AbilityType.Charisma);

    [NotMapped] public int StrengthMod => CalculateModifier(Strength);
    [NotMapped] public int DexterityMod => CalculateModifier(Dexterity);
    [NotMapped] public int ConstitutionMod => CalculateModifier(Constitution);
    [NotMapped] public int IntelligenceMod => CalculateModifier(Intelligence);
    [NotMapped] public int WisdomMod => CalculateModifier(Wisdom);
    [NotMapped] public int CharismaMod => CalculateModifier(Charisma);

    private int CalculateProficiencyBonus()
    {
        var safeLevel = Math.Clamp(Level, 1, 20);
        return 2 + (safeLevel - 1) / 4;
    }

    /// <summary>Gets a character's stored score, defaulting to 10 when it has not been assigned.</summary>
    public int GetAbilityScore(AbilityType ability)
        => AbilityScores.FirstOrDefault(a => a.Ability == ability)?.Value ?? 10;

    /// <summary>Uses floor division so odd scores below 10 produce the correct negative modifier.</summary>
    public static int CalculateModifier(int score)
        => (int)Math.Floor((score - 10) / 2.0);

    public int GetAbilityModifier(AbilityType ability)
        => CalculateModifier(GetAbilityScore(ability));

    public int GetSkillModifier(string skillName)
    {
        var skill = Skills.FirstOrDefault(s =>
            s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

        if (skill is null)
            return 0;

        var abilityModifier = GetAbilityModifier(skill.Ability);
        var proficiency = skill.IsProficient ? ProficiencyBonus : 0;
        var expertise = skill.HasExpertise ? ProficiencyBonus : 0;

        return abilityModifier + proficiency + expertise + skill.MiscModifier;
    }

    /// <summary>
    /// Returns the ability modifier only. Add proficiency when saving-throw proficiency data
    /// is added to the character's class/ruleset model.
    /// </summary>
    public int GetSavingThrowModifier(AbilityType ability)
        => GetAbilityModifier(ability);
}

public enum AbilityType
{
    Strength,
    Dexterity,
    Constitution,
    Intelligence,
    Wisdom,
    Charisma
}
