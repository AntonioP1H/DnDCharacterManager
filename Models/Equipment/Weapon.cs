using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using CommunityToolkit.Mvvm.ComponentModel;

  namespace DnDCharacterManager.Models.Equipment;

  [Table("Weapons")]
  public partial class Weapon : ObservableObject
  {
      public int Id { get; set; }

      [Required, MaxLength(100)]
      public string Name { get; set; } = string.Empty;

      [MaxLength(200)]
      public string? Description { get; set; }

      // Damage
      [Required, MaxLength(20)]
      public string DamageDice { get; set; } = "1d6"; // e.g., "1d8", "2d6"

      [Required, MaxLength(20)]
      public string DamageType { get; set; } = "Slashing"; // Slashing, Piercing, Bludgeoning, Fire, etc.

      // Properties
      [MaxLength(500)]
      public string Properties { get; set; } = string.Empty; // Comma-separated: "Versatile (1d10),Finesse,Light"

      public bool IsMelee { get; set; } = true;
      public bool IsRanged { get; set; } = false;
      public int RangeNormal { get; set; } = 5; // feet
      public int RangeLong { get; set; } = 0;   // 0 = no long range

      // Requirements
      public int MinStrength { get; set; } = 0; // For heavy weapons

      // Special effects (homebrew/custom)
      [MaxLength(200)]
      public string? SpecialEffect { get; set; } // "Burning (1d4 fire, DC 15 Dex save)"
      public bool IsOfficial { get; set; } = true; // Rulebook vs homebrew
      public string? SourceBook { get; set; } // "PHB", "Xanathar's", "Homebrew"
      public int? SourcePage { get; set; }

      // Cost & Weight
      public decimal Cost { get; set; } = 0; // Gold pieces
      public decimal Weight { get; set; } = 0; // Pounds

      // Many-to-many with WeaponProperty
      public virtual ICollection<WeaponProperty> WeaponProperties { get; set; } = new List<WeaponProperty>();

      [NotMapped]
      public string DisplayName => $"{Name} — {DamageDice} {DamageType} damage";

      [NotMapped]
      public IEnumerable<string> PropertyList => Properties.Split(',', StringSplitOptions.RemoveEmptyEntries |
      StringSplitOptions.TrimEntries);
  }