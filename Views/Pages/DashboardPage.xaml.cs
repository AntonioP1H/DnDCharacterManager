using System.Windows.Controls;
using DnDCharacterManager.ViewModels.Pages;

namespace DnDCharacterManager.Views.Pages;

public partial class DashboardPage : Page
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += (_, _) => _viewModel.RefreshCommand.Execute(null);
    }
}
