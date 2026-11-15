using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Characters;

[Table("Races")]
public partial class Race : ObservableObject
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Size { get; set; } = "Medium";

    public int Speed { get; set; } = 30;

    [MaxLength(500)]
    public string AbilityBonuses { get; set; } = "{}";

    [MaxLength(2000)]
    public string Traits { get; set; } = "[]";

    [MaxLength(1000)]
    public string Languages { get; set; } = "[]";

    [MaxLength(500)]
    public string Subraces { get; set; } = "[]";

    [MaxLength(2000)]
    public string SubraceTraits { get; set; } = "{}";

    public bool IsOfficial { get; set; } = true;
    public string? SourceBook { get; set; }
    public int? SourcePage { get; set; }

    [NotMapped]
    public Dictionary<string, int> AbilityBonusDict
    {
        get
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, int>>(AbilityBonuses)
                    ?? new Dictionary<string, int>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, int>();
            }
        }
    }
}
