using System.Windows;
using System.Windows.Controls;

namespace DnDCharacterManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string section)
            return;

        PageTitle.Text = section;
        PageDescription.Text = section switch
        {
            "Dashboard" => "Campaign overview and shortcuts will appear here.",
            "Characters" => "Create, search, edit, and manage your player characters.",
            "Character Creation" => "Build a character using races, classes, abilities, and equipment from your imported rulebooks.",
            "Enemies" => "Browse and manage creatures and custom enemies.",
            "Equipment" => "Browse weapons, armor, and inventory items.",
            "Combat" => "Build encounters, roll initiative, and track combat rounds.",
            "Dice Roller" => "Roll dice expressions and review roll results.",
            "Settings" => "Configure your local database, rulebooks, and application preferences.",
            _ => "Select a section from the navigation menu."
        };
    }
}
