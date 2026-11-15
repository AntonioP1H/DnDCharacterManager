using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Characters;

[Table("Skills")]
public partial class Skill : ObservableObject
{
    public int Id { get; set; }
    public int CharacterId { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public AbilityType Ability { get; set; }

    public bool IsProficient { get; set; }
    public bool HasExpertise { get; set; } // Double proficiency (Rogue, Bard, etc.)

    [Range(-10, 10)]
    public int MiscModifier { get; set; } = 0; // Magic items, feats, etc.

    // Source tracking
    public string? Source { get; set; } // "Class", "Background", "Race", "Feat", "Item"
    public string? SourceDetail { get; set; } // Specific feature name

    [NotMapped]
    public string DisplayName => $"{Name} ({Ability})";
}