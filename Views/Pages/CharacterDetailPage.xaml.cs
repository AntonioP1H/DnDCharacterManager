using System.Windows.Controls;
using DnDCharacterManager.ViewModels.Pages;

namespace DnDCharacterManager.Views.Pages;

public partial class CharacterDetailPage : Page
{
    public CharacterDetailPage(CharacterDetailViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
