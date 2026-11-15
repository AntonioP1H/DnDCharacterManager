using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Characters;

[Table("AbilityScores")]
public partial class AbilityScore : ObservableObject
{
    public int Id { get; set; }
    public int CharacterId { get; set; }

    [Required, MaxLength(20)]
    public AbilityType Ability { get; set; }

    [Range(1, 30)]
    public int Value { get; set; } = 10;

    [Range(-10, 10)]
    public int MiscModifier { get; set; }

    public GenerationMethod Method { get; set; } = GenerationMethod.StandardArray;
    public string? DiceRolls { get; set; }

    [NotMapped]
    public int Modifier => Character.CalculateModifier(Value);

    [NotMapped]
    public int TotalModifier => Modifier + MiscModifier;
}

public enum GenerationMethod
{
    StandardArray,
    PointBuy,
    Roll4d6DropLowest,
    Roll3d6,
    Manual
}
