using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;

namespace DnDCharacterManager.ViewModels.Pages;

public sealed class CombatViewModel : PageViewModelBase
{
    public CombatViewModel(INavigationService navigationService) : base(navigationService)
    {
        Title = "Combat";
    }
}
