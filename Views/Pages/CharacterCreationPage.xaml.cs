using System.Windows.Controls;
using DnDCharacterManager.ViewModels.Pages;

namespace DnDCharacterManager.Views.Pages;

public partial class CharacterCreationPage : Page
{
    public CharacterCreationPage(CharacterCreationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
