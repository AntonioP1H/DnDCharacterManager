using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Enemies;

[Table("EnemySpecialAbilities")]
public partial class EnemySpecialAbility : ObservableObject
{
    public int Id { get; set; }
    public int EnemyId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public AbilityType Type { get; set; } = AbilityType.Passive;
    public int? MaxUses { get; set; }
    public RechargeType Recharge { get; set; } = RechargeType.None;
    public string? RechargeDetail { get; set; }
    public bool IsOfficial { get; set; } = true;
}

public enum AbilityType
{
    Passive = 1,
    Action = 2,
    BonusAction = 3,
    Reaction = 4,
    Legendary = 5,
    Lair = 6
}

public enum RechargeType
{
    None = 0,
    ShortRest = 1,
    LongRest = 2,
    RechargeRoll = 3,
    Daily = 4
}
