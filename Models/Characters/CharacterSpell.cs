using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.Models.Characters;

[Table("CharacterSpells")]
public partial class CharacterSpell : ObservableObject
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public int SpellId { get; set; }

    public bool IsPrepared { get; set; } = true;
    public bool IsKnown { get; set; } = true;
    public int SlotsUsed { get; set; }
}
