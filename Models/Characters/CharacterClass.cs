using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Characters;

[Table("CharacterClasses")]
public partial class CharacterClass : ObservableObject
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public int HitDie { get; set; } = 8;

    [MaxLength(50)]
    public string PrimaryAbility { get; set; } = string.Empty;

    // JSON arrays. Keep these as strings so they map cleanly to a single database column.
    [MaxLength(500)]
    public string SavingThrows { get; set; } = "[]";

    [MaxLength(1000)]
    public string SkillChoices { get; set; } = "[]";

    public int SkillCount { get; set; } = 2;

    [MaxLength(1000)]
    public string ArmorProficiencies { get; set; } = "[]";

    [MaxLength(1000)]
    public string WeaponProficiencies { get; set; } = "[]";

    [MaxLength(1000)]
    public string ToolProficiencies { get; set; } = "[]";

    [MaxLength(2000)]
    public string ClassFeatures { get; set; } = "[]";

    [MaxLength(2000)]
    public string Subclasses { get; set; } = "[]";

    [MaxLength(50)]
    public string SpellcastingAbility { get; set; } = string.Empty;

    // 0 = none, 1 = full, 2 = half, 3 = one-third progression.
    public int SpellcastingLevel { get; set; }

    public bool IsOfficial { get; set; } = true;
    public string? SourceBook { get; set; }
    public int? SourcePage { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [NotMapped]
    public string[] SavingThrowArray => DeserializeArray(SavingThrows);

    [NotMapped]
    public string[] SkillChoicesArray => DeserializeArray(SkillChoices);

    private static string[] DeserializeArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<string>();

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }
}
