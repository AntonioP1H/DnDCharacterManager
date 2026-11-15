using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Models.Equipment;

namespace DnDCharacterManager.Models.Enemies;

[Table("Enemies")]
public partial class Enemy : ObservableObject
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string CreatureType { get; set; } = "Humanoid";

    [MaxLength(20)]
    public string ChallengeRating { get; set; } = "1/4";

    public int ProficiencyBonus { get; set; } = 2;

    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    public int MaxHitPoints { get; set; }
    public string? HitDice { get; set; }
    public int ArmorClass { get; set; }
    public string? ArmorType { get; set; }

    public int SpeedWalk { get; set; } = 30;
    public int SpeedFly { get; set; }
    public int SpeedSwim { get; set; }
    public int SpeedClimb { get; set; }

    [MaxLength(200)] public string SavingThrowProficiencies { get; set; } = "[]";
    [MaxLength(500)] public string SkillProficiencies { get; set; } = "[]";
    [MaxLength(500)] public string DamageResistances { get; set; } = "[]";
    [MaxLength(500)] public string DamageImmunities { get; set; } = "[]";
    [MaxLength(500)] public string DamageVulnerabilities { get; set; } = "[]";
    [MaxLength(500)] public string ConditionImmunities { get; set; } = "[]";

    public int PassivePerception { get; set; } = 10;
    public string? Senses { get; set; }

    [MaxLength(500)] public string Languages { get; set; } = "[]";

    public bool IsOfficial { get; set; } = true;
    public string? SourceBook { get; set; }
    public int? SourcePage { get; set; }

    public virtual ICollection<EnemyAttack> Attacks { get; set; } = new List<EnemyAttack>();
    public virtual ICollection<EnemySpecialAbility> SpecialAbilities { get; set; } = new List<EnemySpecialAbility>();

    [NotMapped] public int StrengthMod => Character.CalculateModifier(Strength);
    [NotMapped] public int DexterityMod => Character.CalculateModifier(Dexterity);
    [NotMapped] public int ConstitutionMod => Character.CalculateModifier(Constitution);
    [NotMapped] public int IntelligenceMod => Character.CalculateModifier(Intelligence);
    [NotMapped] public int WisdomMod => Character.CalculateModifier(Wisdom);
    [NotMapped] public int CharismaMod => Character.CalculateModifier(Charisma);
    [NotMapped] public int InitiativeModifier => DexterityMod;
}
