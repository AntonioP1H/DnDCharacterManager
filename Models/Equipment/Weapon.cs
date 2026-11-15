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

    [Required, MaxLength(20)]
    public string DamageDice { get; set; } = "1d6";

    [Required, MaxLength(20)]
    public string DamageType { get; set; } = "Slashing";

    [MaxLength(500)]
    public string Properties { get; set; } = string.Empty;

    public bool IsMelee { get; set; } = true;
    public bool IsRanged { get; set; }
    public int RangeNormal { get; set; } = 5;
    public int RangeLong { get; set; }
    public int MinStrength { get; set; }

    [MaxLength(200)]
    public string? SpecialEffect { get; set; }

    public bool IsOfficial { get; set; } = true;
    public string? SourceBook { get; set; }
    public int? SourcePage { get; set; }
    public decimal Cost { get; set; }
    public decimal Weight { get; set; }

    public virtual ICollection<WeaponProperty> WeaponProperties { get; set; } = new List<WeaponProperty>();

    [NotMapped]
    public string DisplayName => $"{Name} — {DamageDice} {DamageType} damage";

    [NotMapped]
    public IEnumerable<string> PropertyList => Properties.Split(
        ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
