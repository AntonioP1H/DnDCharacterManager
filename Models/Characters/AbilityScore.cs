using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using CommunityToolkit.Mvvm.ComponentModel;

  namespace DnDCharacterManager.Models.Characters;

  [Table("AbilityScores")]
  public partial class AbilityScore : ObservableObject
  {
      public int Id { get; set; }
      public int CharacterId { get; set; }

      [Required, MaxLength(20)]
      public AbilityType Ability { get; set; }

      [Range(1, 30)]
      public int Value { get; set; } = 10;

      [Range(0, 10)]
      public int MiscModifier { get; set; } = 0;

      // Generation method tracking
      public GenerationMethod Method { get; set; } = GenerationMethod.StandardArray;
      public string? DiceRolls { get; set; } // JSON: "[15,14,13,12,10,8]"

      // Calculated (not stored)
      [NotMapped]
      public int Modifier => (Value - 10) / 2;

      [NotMapped]
      public int TotalModifier => Modifier + MiscModifier;
  }

  public enum GenerationMethod
  {
      StandardArray,      // 15,14,13,12,10,8
      PointBuy,           // 27 point buy
      Roll4d6DropLowest,  // 4d6 drop lowest ×6
      Roll3d6,            // 3d6 ×6
      Manual              // Manual entry
  }

  