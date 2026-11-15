using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Services.Data.Repositories;
using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;

namespace DnDCharacterManager.ViewModels.Pages;

public sealed record DashboardStatCard(string Label, string Value, PackIconKind Icon);

public partial class DashboardViewModel : PageViewModelBase
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IEnemyRepository _enemyRepository;
    private readonly IEncounterRepository _encounterRepository;

    [ObservableProperty]
    private int _characterCount;

    [ObservableProperty]
    private int _enemyCount;

    [ObservableProperty]
    private int _encounterCount;

    [ObservableProperty]
    private Character? _recentCharacter;

    public ObservableCollection<DashboardStatCard> StatCards { get; } = new();

    public DashboardViewModel(
        INavigationService navigationService,
        ICharacterRepository characterRepository,
        IEnemyRepository enemyRepository,
        IEncounterRepository encounterRepository)
        : base(navigationService)
    {
        _characterRepository = characterRepository;
        _enemyRepository = enemyRepository;
        _encounterRepository = encounterRepository;
        Title = "Dashboard";

        StatCards.Add(new DashboardStatCard("CHARACTERS", "0", PackIconKind.AccountGroup));
        StatCards.Add(new DashboardStatCard("ENEMIES", "0", PackIconKind.Skull));
        StatCards.Add(new DashboardStatCard("ENCOUNTERS", "0", PackIconKind.DiceD20));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = "Loading dashboard...";

        try
        {
            var characters = await _characterRepository.GetAllAsync();
            var enemies = await _enemyRepository.GetAllAsync();
            var encounters = await _encounterRepository.GetAllAsync();

            CharacterCount = characters.Count;
            EnemyCount = enemies.Count;
            EncounterCount = encounters.Count;
            RecentCharacter = characters.OrderByDescending(c => c.Id).FirstOrDefault();

            StatCards[0] = new DashboardStatCard("CHARACTERS", CharacterCount.ToString(), PackIconKind.AccountGroup);
            StatCards[1] = new DashboardStatCard("ENEMIES", EnemyCount.ToString(), PackIconKind.Skull);
            StatCards[2] = new DashboardStatCard("ENCOUNTERS", EncounterCount.ToString(), PackIconKind.DiceD20);
            StatusMessage = "Dashboard updated.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NewCharacter() => NavigationService.NavigateTo("CharacterCreation");

    [RelayCommand]
    private void ViewCharacters() => NavigationService.NavigateTo("Characters");

    [RelayCommand]
    private void NewEncounter() => NavigationService.NavigateTo("Combat");

    [RelayCommand]
    private void OpenDiceRoller() => NavigationService.NavigateTo("DiceRoller");

    [RelayCommand]
    private void OpenEquipment() => NavigationService.NavigateTo("Equipment");
}
