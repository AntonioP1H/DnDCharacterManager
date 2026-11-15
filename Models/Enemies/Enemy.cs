using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using CommunityToolkit.Mvvm.ComponentModel;
  using DnDCharacterManager.Models.Equipment;

  namespace DnDCharacterManager.Models.Enemies;

  [Table("Enemies")]
  public partial class Enemy : ObservableObject
  {
      public int Id { get; set; }

      [Required, MaxLength(100)]
      public string Name { get; set; } = string.Empty;

      [MaxLength(50)]
      public string CreatureType { get; set; } = "Humanoid"; // Beast, Dragon, Undead, Fiend, etc.

      [MaxLength(20)]
      public string ChallengeRating { get; set; } = "1/4"; // "1/8", "1/4", "1/2", "1".."30"

      public int ProficiencyBonus { get; set; } = 2;

      // Ability Scores
      public int Strength { get; set; } = 10;
      public int Dexterity { get; set; } = 10;
      public int Constitution { get; set; } = 10;
      public int Intelligence { get; set; } = 10;
      public int Wisdom { get; set; } = 10;
      public int Charisma { get; set; } = 10;

      // HP & AC
      public int MaxHitPoints { get; set; }
      public string? HitDice { get; set; } // "2d6+2"
      public int ArmorClass { get; set; }
      public string? ArmorType { get; set; } // "Natural armor", "Leather", etc.

      // Speed
      public int SpeedWalk { get; set; } = 30;
      public int SpeedFly { get; set; } = 0;
      public int SpeedSwim { get; set; } = 0;
      public int SpeedClimb { get; set; } = 0;

      // Saving Throws (proficient ones)
      [MaxLength(200)]
      public string SavingThrowProficiencies { get; set; } = "[]"; // JSON array of ability names

      // Skills
      [MaxLength(500)]
      public string SkillProficiencies { get; set; } = "[]"; // JSON: [{"name":"Stealth","bonus":6}]

      // Resistances/Immunities/Vulnerabilities
      [MaxLength(500)]
      public string DamageResistances { get; set; } = "[]";
      [MaxLength(500)]
      public string DamageImmunities { get; set; } = "[]";
      [MaxLength(500)]
      public string DamageVulnerabilities { get; set; } = "[]";
      [MaxLength(500)]
      public string ConditionImmunities { get; set; } = "[]";

      // Senses
      public int PassivePerception { get; set; } = 10;
      public string? Senses { get; set; } // "Darkvision 60 ft."

      // Languages
      [MaxLength(500)]
      public string Languages { get; set; } = "[]";

      // Source
      public bool IsOfficial { get; set; } = true;
      public string? SourceBook { get; set; }
      public int? SourcePage { get; set; }

      // Navigation
      public virtual ICollection<EnemyAttack> Attacks { get; set; } = new List<EnemyAttack>();
      public virtual ICollection<EnemySpecialAbility> SpecialAbilities { get; set; } = new List<EnemySpecialAbility>();

      // Calculated
      [NotMapped]
      public int StrengthMod => (Strength - 10) / 2;
      [NotMapped]
      public int DexterityMod => (Dexterity - 10) / 2;
      [NotMapped]
      public int ConstitutionMod => (Constitution - 10) / 2;
      [NotMapped]
      public int IntelligenceMod => (Intelligence - 10) / 2;
      [NotMapped]
      public int WisdomMod => (Wisdom - 10) / 2;
      [NotMapped]
      public int CharismaMod => (Charisma - 10) / 2;

      [NotMapped]
      public int InitiativeModifier => DexterityMod;
  }