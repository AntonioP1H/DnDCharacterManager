 using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using CommunityToolkit.Mvvm.ComponentModel;

  namespace DnDCharacterManager.Models.Equipment;

  [Table("Armors")]
  public partial class Armor : ObservableObject
  {
      public int Id { get; set; }

      [Required, MaxLength(100)]
      public string Name { get; set; } = string.Empty;

      [MaxLength(200)]
      public string? Description { get; set; }

      [Required, MaxLength(20)]
      public string Category { get; set; } = "Light"; // Light, Medium, Heavy, Shield

      // AC Calculation
      [Required, MaxLength(50)]
      public string ArmorClassFormula { get; set; } = "11 + Dex"; // e.g., "13 + Dex (max 2)", "16", "10 + Dex"

      public int BaseAC { get; set; } = 11;
      public int? MaxDexBonus { get; set; } = null; // null = no limit
      public int MinStrength { get; set; } = 0;
      public bool StealthDisadvantage { get; set; } = false;

      // Cost & Weight
      public decimal Cost { get; set; } = 0;
      public decimal Weight { get; set; } = 0;

      // Source
      public bool IsOfficial { get; set; } = true;
      public string? SourceBook { get; set; }
      public int? SourcePage { get; set; }

      [NotMapped]
      public string DisplayName => $"{Name} ({Category}) — AC: {ArmorClassFormula}";
  }