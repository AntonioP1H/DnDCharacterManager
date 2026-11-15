using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Characters;

[Table("Backgrounds")]
public partial class Background : ObservableObject
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string SkillProficiencies { get; set; } = "[]";

    public int Languages { get; set; }

    [MaxLength(500)]
    public string ToolProficiencies { get; set; } = "[]";

    [MaxLength(1000)]
    public string Equipment { get; set; } = "[]";

    [MaxLength(100)]
    public string Feature { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string FeatureDescription { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string PersonalityTraits { get; set; } = "[]";

    [MaxLength(1000)]
    public string Ideals { get; set; } = "[]";

    [MaxLength(1000)]
    public string Bonds { get; set; } = "[]";

    [MaxLength(1000)]
    public string Flaws { get; set; } = "[]";

    public bool IsOfficial { get; set; } = true;
    public string? SourceBook { get; set; }
    public int? SourcePage { get; set; }
}
