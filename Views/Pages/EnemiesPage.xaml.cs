using System.Windows.Controls;
using DnDCharacterManager.ViewModels.Pages;

namespace DnDCharacterManager.Views.Pages;

public partial class EnemiesPage : Page
{
    public EnemiesPage(EnemiesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
