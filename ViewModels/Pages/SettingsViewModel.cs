using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;

namespace DnDCharacterManager.ViewModels.Pages;

public sealed class SettingsViewModel : PageViewModelBase
{
    public SettingsViewModel(INavigationService navigationService) : base(navigationService)
    {
        Title = "Settings";
    }
}
