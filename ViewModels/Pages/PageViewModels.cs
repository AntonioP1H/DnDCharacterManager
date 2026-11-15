using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.ViewModels.Pages;

public abstract class PageViewModelBase : ObservableObject
{
    private string _title;

    protected PageViewModelBase(string title) => _title = title;

    public string Title
    {
        get => _title;
        protected set => SetProperty(ref _title, value);
    }
}

public sealed class DashboardViewModel : PageViewModelBase
{
    public DashboardViewModel() : base("Dashboard") { }
}

public sealed class CharactersViewModel : PageViewModelBase
{
    public CharactersViewModel() : base("Characters") { }
}

public sealed class CharacterCreationViewModel : PageViewModelBase
{
    public CharacterCreationViewModel() : base("Create Character") { }
}

public sealed class CharacterDetailViewModel : PageViewModelBase
{
    public CharacterDetailViewModel() : base("Character Details") { }
}

public sealed class EnemiesViewModel : PageViewModelBase
{
    public EnemiesViewModel() : base("Enemies") { }
}

public sealed class EquipmentViewModel : PageViewModelBase
{
    public EquipmentViewModel() : base("Equipment") { }
}

public sealed class CombatViewModel : PageViewModelBase
{
    public CombatViewModel() : base("Combat") { }
}

public sealed class DiceRollerViewModel : PageViewModelBase
{
    public DiceRollerViewModel() : base("Dice Roller") { }
}

public sealed class SettingsViewModel : PageViewModelBase
{
    public SettingsViewModel() : base("Settings") { }
}
