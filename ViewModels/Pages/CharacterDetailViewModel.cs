using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;

namespace DnDCharacterManager.ViewModels.Pages;

public sealed class CharacterDetailViewModel : PageViewModelBase
{
    public CharacterDetailViewModel(INavigationService navigationService) : base(navigationService)
    {
        Title = "Character Details";
    }
}
