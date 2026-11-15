 using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using CommunityToolkit.Mvvm.ComponentModel;

  namespace DnDCharacterManager.Models.Equipment;

  [Table("DamageTypes")]
  public partial class DamageType : ObservableObject
  {
      public int Id { get; set; }

      [Required, MaxLength(20)]
      public string Name { get; set; } = string.Empty;

      [MaxLength(200)]
      public string? Description { get; set; }
  }

  public enum DamageTypeEnum
  {
      Slashing = 1,
      Piercing = 2,
      Bludgeoning = 3,
      Fire = 4,
      Cold = 5,
      Lightning = 6,
      Thunder = 7,
      Acid = 8,
      Poison = 9,
      Psychic = 10,
      Radiant = 11,
      Necrotic = 12,
      Force = 13
  }