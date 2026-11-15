using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;

namespace DnDCharacterManager.ViewModels.Pages;

public sealed class EnemiesViewModel : PageViewModelBase
{
    public EnemiesViewModel(INavigationService navigationService) : base(navigationService)
    {
        Title = "Enemies";
    }
}
