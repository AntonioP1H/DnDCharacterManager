using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;

namespace DnDCharacterManager.ViewModels.Pages;

public sealed class CharactersViewModel : PageViewModelBase
{
    public CharactersViewModel(INavigationService navigationService) : base(navigationService)
    {
        Title = "Characters";
    }
}
