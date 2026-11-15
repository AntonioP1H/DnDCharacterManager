using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using CommunityToolkit.Mvvm.ComponentModel;

  namespace DnDCharacterManager.Models.Combat;

  [Table("CombatParticipants")]
  public partial class CombatParticipant : ObservableObject
  {
      public int Id { get; set; }
      public int EncounterId { get; set; }

      [Required, MaxLength(100)]
      public string Name { get; set; } = string.Empty;

      public ParticipantType Type { get; set; } = ParticipantType.Enemy;

      // Source references
      public int? CharacterId { get; set; }
      public int? EnemyId { get; set; }

      // Combat stats
      public int MaxHitPoints { get; set; }
      public int CurrentHitPoints { get; set; }
      public int TemporaryHitPoints { get; set; }
      public int ArmorClass { get; set; }
      public int Initiative { get; set; }
      public int InitiativeModifier { get; set; }
      public int InitiativeRoll { get; set; }

      // Status
      public bool IsUnconscious { get; set; }
      public bool IsDead { get; set; }
      public bool IsConcentrating { get; set; }
      public string? ConcentrationSpell { get; set; }

      // Conditions (JSON serialized list of condition IDs)
      [MaxLength(500)]
      public string ActiveConditions { get; set; } = "[]";

      public int SortOrder { get; set; } // For tie-breaking

      [NotMapped]
      public string DisplayName => Type == ParticipantType.PlayerCharacter ? $"🛡 {Name}" : $"⚔ {Name}";
  }

  public enum ParticipantType
  {
      PlayerCharacter = 1,
      Enemy = 2,
      NPC = 3
  }