using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DnDCharacterManager.Models.Characters;

namespace DnDCharacterManager.Models.Equipment;

/// <summary>An inventory row linking a character to a catalogue equipment item.</summary>
[Table("CharacterEquipment")]
public class CharacterEquipment
{
    [Key] public int Id { get; set; }
    public int CharacterId { get; set; }
    public virtual Character Character { get; set; } = null!;
    public int? EquipmentItemId { get; set; }
    public virtual EquipmentItem? Item { get; set; }
    public int Quantity { get; set; } = 1;
    public bool IsEquipped { get; set; }
    public bool IsAttuned { get; set; }
}

/// <summary>A weapon instance owned by a character.</summary>
[Table("CharacterWeapons")]
public class CharacterWeapon
{
    [Key] public int Id { get; set; }
    public int CharacterId { get; set; }
    public virtual Character Character { get; set; } = null!;
    public int? WeaponId { get; set; }
    public virtual Weapon? Weapon { get; set; }
    public int Quantity { get; set; } = 1;
    public bool IsEquipped { get; set; }
    public bool IsProficient { get; set; }
}

/// <summary>An armor instance owned by a character.</summary>
[Table("CharacterArmor")]
public class CharacterArmor
{
    [Key] public int Id { get; set; }
    public int CharacterId { get; set; }
    public virtual Character Character { get; set; } = null!;
    public int? ArmorId { get; set; }
    public virtual Armor? Armor { get; set; }
    public bool IsEquipped { get; set; }
}
