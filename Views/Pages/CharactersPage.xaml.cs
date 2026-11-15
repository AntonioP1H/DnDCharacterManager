using System.Windows.Controls;
using DnDCharacterManager.ViewModels.Pages;

namespace DnDCharacterManager.Views.Pages;

public partial class CharactersPage : Page
{
    public CharactersPage(CharactersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
