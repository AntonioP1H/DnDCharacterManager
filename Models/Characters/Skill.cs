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
    public bool HasExpertise { get; set; }

    [Range(-10, 10)]
    public int MiscModifier { get; set; }

    public string? Source { get; set; }
    public string? SourceDetail { get; set; }

    [NotMapped]
    public string DisplayName => $"{Name} ({Ability})";
}
