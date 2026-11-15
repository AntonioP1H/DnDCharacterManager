using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Equipment;

[Table("WeaponProperties")]
public partial class WeaponProperty : ObservableObject
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public virtual ICollection<Weapon> Weapons { get; set; } = new List<Weapon>();
}

public enum WeaponPropertyEnum
{
    Light = 1,
    Heavy = 2,
    Finesse = 3,
    Reach = 4,
    Thrown = 5,
    Versatile = 6,
    Loading = 7,
    Ammunition = 8,
    TwoHanded = 9,
    Special = 10
}
