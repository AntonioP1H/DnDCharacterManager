using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;

namespace DnDCharacterManager.ViewModels.Pages;

public sealed class DiceRollerViewModel : PageViewModelBase
{
    public DiceRollerViewModel(INavigationService navigationService) : base(navigationService)
    {
        Title = "Dice Roller";
    }
}
