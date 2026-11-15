using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Equipment;

[Table("Conditions")]
public partial class Condition : ObservableObject
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public bool GivesAdvantageToAttackers { get; set; }
    public bool GivesDisadvantageToAttacks { get; set; }
    public bool GivesDisadvantageToAbilityChecks { get; set; }
    public bool PreventsMovement { get; set; }
    public bool PreventsActions { get; set; }
    public bool PreventsReactions { get; set; }
    public bool AutoFailStrengthDexSaves { get; set; }
    public bool AttackersHaveAdvantage { get; set; }
}

public enum ConditionEnum
{
    Blinded = 1,
    Charmed = 2,
    Deafened = 3,
    Exhaustion = 4,
    Frightened = 5,
    Grappled = 6,
    Incapacitated = 7,
    Invisible = 8,
    Paralyzed = 9,
    Petrified = 10,
    Poisoned = 11,
    Prone = 12,
    Restrained = 13,
    Stunned = 14,
    Unconscious = 15
}
