using System.Windows.Controls;
using DnDCharacterManager.ViewModels.Pages;

namespace DnDCharacterManager.Views.Pages;

public partial class DiceRollerPage : Page
{
    public DiceRollerPage(DiceRollerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
