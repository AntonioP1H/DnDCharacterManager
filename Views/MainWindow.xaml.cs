using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;
using DnDCharacterManager.Views.Pages;

namespace DnDCharacterManager.Views;

public partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    private readonly (string Key, string Name, string Icon, Type PageType)[] _navItems =
    [
        ("Dashboard", "Dashboard", "ViewDashboard", typeof(DashboardPage)),
        ("Characters", "Characters", "AccountGroup", typeof(CharactersPage)),
        ("CharacterCreation", "Create Character", "AccountPlus", typeof(CharacterCreationPage)),
        ("Enemies", "Enemies", "Skull", typeof(EnemiesPage)),
        ("Equipment", "Equipment", "SwordCross", typeof(EquipmentPage)),
        ("Combat", "Combat", "DiceD20", typeof(CombatPage)),
        ("DiceRoller", "Dice Roller", "DiceMultiple", typeof(DiceRollerPage)),
        ("Settings", "Settings", "Cog", typeof(SettingsPage))
    ];

    public MainWindow(INavigationService navigationService)
    {
        InitializeComponent();
        _navigationService = navigationService;
        _navigationService.SetFrame(ContentFrame);
        _navigationService.Navigated += OnNavigated;

        foreach (var item in _navItems)
            _navigationService.RegisterPage(item.Key, item.PageType);

        BuildNavigationMenu();
        _navigationService.NavigateTo("Dashboard");
    }

    private void BuildNavigationMenu()
    {
        foreach (var item in _navItems)
            NavMenuPanel.Children.Add(CreateNavButton(item.Key, item.Name, item.Icon));
    }

    private Button CreateNavButton(string key, string name, string iconName)
    {
        PackIconKind iconKind = Enum.TryParse(iconName, true, out PackIconKind parsed)
            ? parsed
            : PackIconKind.HelpCircle;

        var button = new Button
        {
            Style = (Style)FindResource("FantasyButton"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(16, 12, 0, 0),
            Margin = new Thickness(4, 2, 0, 0),
            Tag = key,
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new PackIcon
                    {
                        Kind = iconKind,
                        Width = 22,
                        Height = 22,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 12, 0)
                    },
                    new TextBlock
                    {
                        Text = name,
                        Style = (Style)FindResource("BodyText"),
                        FontSize = 15,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };

        button.Click += (_, _) => _navigationService.NavigateTo(key);
        return button;
    }

    private void OnNavigated(Page? page)
    {
        PageTitleText.Text = page?.DataContext is ViewModelBase viewModel
            ? viewModel.Title
            : page?.Title ?? "D&D Manager";
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
        => _navigationService.NavigateTo("Settings");

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "D&D Character Manager v0.1.0\n\n" +
            "Local-first desktop application.\n" +
            "Rules and game content are imported from your own rulebooks.\n\n" +
            "This build provides the application foundation and dashboard.",
            "About D&D Character Manager",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
