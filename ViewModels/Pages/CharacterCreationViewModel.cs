using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;

namespace DnDCharacterManager.ViewModels.Pages;

public sealed class CharacterCreationViewModel : PageViewModelBase
{
    public CharacterCreationViewModel(INavigationService navigationService) : base(navigationService)
    {
        Title = "Create Character";
    }
}
