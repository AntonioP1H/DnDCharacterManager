using System.Windows.Controls;
using DnDCharacterManager.ViewModels.Pages;

namespace DnDCharacterManager.Views.Pages;

public partial class EquipmentPage : Page
{
    public EquipmentPage(EquipmentViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
