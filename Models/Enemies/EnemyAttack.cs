using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Enemies;

[Table("EnemyAttacks")]
public partial class EnemyAttack : ObservableObject
{
    public int Id { get; set; }
    public int EnemyId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public AttackType Type { get; set; } = AttackType.MeleeWeapon;
    public int Reach { get; set; } = 5;
    public int RangeNormal { get; set; }
    public int RangeLong { get; set; }
    public int AttackBonus { get; set; }

    [Required, MaxLength(50)]
    public string DamageDice { get; set; } = "1d6";

    [Required, MaxLength(20)]
    public string DamageType { get; set; } = "Slashing";

    [MaxLength(500)]
    public string? SpecialEffect { get; set; }

    public bool IsOfficial { get; set; } = true;

    [NotMapped]
    public string DisplayText => $"{Name}: {AttackBonus:+#;-#;0} to hit, reach {Reach} ft., {DamageDice} {DamageType} damage";
}

public enum AttackType
{
    MeleeWeapon = 1,
    RangedWeapon = 2,
    MeleeSpell = 3,
    RangedSpell = 4,
    Multiattack = 5
}
