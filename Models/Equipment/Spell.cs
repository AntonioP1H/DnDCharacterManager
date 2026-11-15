using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Equipment;

[Table("Spells")]
public partial class Spell : ObservableObject
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int Level { get; set; } // 0 = cantrip.

    [MaxLength(30)]
    public string School { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CastingTime { get; set; } = "1 action";

    [MaxLength(100)]
    public string Range { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Components { get; set; } = "V, S";

    [MaxLength(200)]
    public string MaterialComponent { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Duration { get; set; } = "Instantaneous";

    public bool Concentration { get; set; }
    public bool Ritual { get; set; }

    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string HigherLevels { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Classes { get; set; } = "[]";

    [MaxLength(500)]
    public string Subclasses { get; set; } = "[]";

    public bool IsOfficial { get; set; } = true;
    public string? SourceBook { get; set; }
    public int? SourcePage { get; set; }

    [NotMapped]
    public string[] ClassArray
    {
        get
        {
            try
            {
                return JsonSerializer.Deserialize<string[]>(Classes) ?? Array.Empty<string>();
            }
            catch (JsonException)
            {
                return Array.Empty<string>();
            }
        }
    }

    [NotMapped]
    public string DisplayName => Level == 0 ? $"{Name} (Cantrip)" : $"{Name} (Level {Level})";
}
