using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using DnDCharacterManager.Models.Equipment;

namespace DnDCharacterManager.Models.Characters;

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
        int safeLevel = Math.Clamp(Level, 1, 20);
        return 2 + (safeLevel - 1) / 4;
    }

    public int GetAbilityScore(AbilityType ability)
        => AbilityScores.FirstOrDefault(a => a.Ability == ability)?.Value ?? 10;

    public static int CalculateModifier(int score)
        => (int)Math.Floor((score - 10) / 2.0);

    public int GetAbilityModifier(AbilityType ability)
        => CalculateModifier(GetAbilityScore(ability));

    public int GetSkillModifier(string skillName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(skillName);
        var skill = Skills.FirstOrDefault(s =>
            s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

        if (skill is null)
            return 0;

        int proficiency = skill.HasExpertise
            ? ProficiencyBonus * 2
            : skill.IsProficient ? ProficiencyBonus : 0;

        return GetAbilityModifier(skill.Ability) + proficiency + skill.MiscModifier;
    }

    public int GetSavingThrowModifier(AbilityType ability)
    {
        // Saving throw proficiency is not yet represented in this model.
        return GetAbilityModifier(ability);
    }
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
