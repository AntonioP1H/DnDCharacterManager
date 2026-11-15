using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Combat;

[Table("CombatLog")]
public partial class CombatLogEntry : ObservableObject
{
    public int Id { get; set; }
    public int EncounterId { get; set; }
    public int Round { get; set; }
    public int TurnIndex { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    [Required, MaxLength(100)]
    public string ActorName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string ActionType { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? DiceExpression { get; set; }
    public int? DiceResult { get; set; }
    public int? TotalResult { get; set; }
    public int? DamageDealt { get; set; }
    public string? DamageType { get; set; }
    public string? TargetName { get; set; }
}
