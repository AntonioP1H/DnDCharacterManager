using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Equipment;

[Table("EquipmentItems")]
public partial class EquipmentItem : ObservableObject
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string Category { get; set; } = "Adventuring Gear";

    public decimal Cost { get; set; }
    public decimal Weight { get; set; }

    [MaxLength(1000)]
    public string? Properties { get; set; }

    public bool IsOfficial { get; set; } = true;
    public string? SourceBook { get; set; }
    public int? SourcePage { get; set; }
}
