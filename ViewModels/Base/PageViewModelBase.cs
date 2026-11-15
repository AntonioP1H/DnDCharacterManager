using DnDCharacterManager.Services.Navigation;

namespace DnDCharacterManager.ViewModels.Base;

public abstract partial class PageViewModelBase : ViewModelBase
{
    protected INavigationService NavigationService { get; }

    protected PageViewModelBase(INavigationService navigationService)
        => NavigationService = navigationService;
}
