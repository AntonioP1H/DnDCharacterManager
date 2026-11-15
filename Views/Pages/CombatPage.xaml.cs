using System.Windows.Controls;
using DnDCharacterManager.ViewModels.Pages;

namespace DnDCharacterManager.Views.Pages;

public partial class CombatPage : Page
{
    public CombatPage(CombatViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
