using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;

namespace DnDCharacterManager.ViewModels.Pages;

public sealed class EquipmentViewModel : PageViewModelBase
{
    public EquipmentViewModel(INavigationService navigationService) : base(navigationService)
    {
        Title = "Equipment";
    }
}
