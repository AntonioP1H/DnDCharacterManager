using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Equipment;

[Table("Armors")]
public partial class Armor : ObservableObject
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    [Required, MaxLength(20)]
    public string Category { get; set; } = "Light";

    [Required, MaxLength(50)]
    public string ArmorClassFormula { get; set; } = "11 + Dex";

    public int BaseAC { get; set; } = 11;
    public int? MaxDexBonus { get; set; }
    public int MinStrength { get; set; }
    public bool StealthDisadvantage { get; set; }
    public decimal Cost { get; set; }
    public decimal Weight { get; set; }
    public bool IsOfficial { get; set; } = true;
    public string? SourceBook { get; set; }
    public int? SourcePage { get; set; }

    [NotMapped]
    public string DisplayName => $"{Name} ({Category}) — AC: {ArmorClassFormula}";
}
