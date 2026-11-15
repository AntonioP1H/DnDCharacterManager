using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using CommunityToolkit.Mvvm.ComponentModel;

  namespace DnDCharacterManager.Models.Combat;

  [Table("Encounters")]
  public partial class Encounter : ObservableObject
  {
      public int Id { get; set; }

      [Required, MaxLength(100)]
      public string Name { get; set; } = string.Empty;

      public DateTime CreatedAt { get; set; } = DateTime.Now;
      public DateTime? LastPlayedAt { get; set; }

      public int CurrentRound { get; set; } = 0;
      public int CurrentTurnIndex { get; set; } = 0;
      public bool IsActive { get; set; } = false;

      public virtual ICollection<CombatParticipant> Participants { get; set; } = new List<CombatParticipant>();
      public virtual ICollection<CombatLogEntry> CombatLog { get; set; } = new List<CombatLogEntry>();

      [NotMapped]
      public CombatParticipant? CurrentParticipant =>
          Participants.OrderByDescending(p => p.Initiative)
                     .ThenBy(p => p.Id)
                     .ElementAtOrDefault(CurrentTurnIndex);
  }