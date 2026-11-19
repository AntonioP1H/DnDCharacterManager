using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Services.Data.Repositories;
using DnDCharacterManager.Services.Import;
using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.ViewModels.Base;

namespace DnDCharacterManager.ViewModels.Pages;

public sealed record DashboardStatCard(string Label, string Value, PackIconKind Icon);

public partial class DashboardViewModel : PageViewModelBase
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IEnemyRepository _enemyRepository;
    private readonly IEncounterRepository _encounterRepository;
    private readonly IRulebookImporter _rulebookImporter;

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
        IEncounterRepository encounterRepository,
        IRulebookImporter rulebookImporter)
        : base(navigationService)
    {
        _characterRepository = characterRepository;
        _enemyRepository = enemyRepository;
        _encounterRepository = encounterRepository;
        _rulebookImporter = rulebookImporter;
        Title = "Dashboard";

        StatCards.Add(new DashboardStatCard("CHARACTERS", "0", PackIconKind.AccountGroup));
        StatCards.Add(new DashboardStatCard("ENEMIES", "0", PackIconKind.Skull));
        StatCards.Add(new DashboardStatCard("ENCOUNTERS", "0", PackIconKind.DiceD20));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        StatusMessage = "Loading dashboard...";
        try
        {
            await RefreshDashboardDataAsync();
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
    private async Task ImportRulebooksAsync()
    {
        if (IsBusy)
            return;

        var basicRulesDialog = CreatePdfDialog("Select the 5e Basic Rules PDF");
        if (basicRulesDialog.ShowDialog() != true)
        {
            StatusMessage = "Rulebook import cancelled.";
            return;
        }

        var monsterManualDialog = CreatePdfDialog("Select the 5e Monster Manual PDF");
        if (monsterManualDialog.ShowDialog() != true)
        {
            StatusMessage = "Rulebook import cancelled. The Monster Manual is required.";
            return;
        }

        var legacyPhbDialog = CreatePdfDialog("Select the 2e Player's Handbook PDF (optional — Cancel to skip)");
        bool importLegacyPhb = legacyPhbDialog.ShowDialog() == true;

        IsBusy = true;
        StatusMessage = "Starting rulebook catalogue import...";
        string lastProgressMessage = string.Empty;
        string? catalogueTotalsMessage = null;

        // This reporter runs synchronously on the UI thread, so each progress message is visible in StatusMessage.
        IProgress<string> progress = new InlineProgress<string>(message =>
        {
            lastProgressMessage = message;
            if (message.StartsWith("Database totals", StringComparison.Ordinal))
                catalogueTotalsMessage = message;
            StatusMessage = message;
        });

        try
        {
            await _rulebookImporter.ImportBasicRulesAsync(basicRulesDialog.FileName, progress);
            await _rulebookImporter.ImportMonsterManualAsync(monsterManualDialog.FileName, progress);

            if (importLegacyPhb)
                await _rulebookImporter.ImportPhb2eAsync(legacyPhbDialog.FileName, progress);

            // Do not call RefreshAsync here: it exits while IsBusy is true. Refresh the data directly.
            await RefreshDashboardDataAsync();
            string summary = catalogueTotalsMessage ?? lastProgressMessage;
            StatusMessage = string.IsNullOrWhiteSpace(summary)
                ? "Rulebook catalogue import completed and dashboard refreshed."
                : $"Import completed and dashboard refreshed. {summary}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Rulebook import failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static OpenFileDialog CreatePdfDialog(string title) => new()
    {
        Title = title,
        Filter = "PDF files (*.pdf)|*.pdf",
        DefaultExt = ".pdf",
        AddExtension = true,
        CheckFileExists = true,
        Multiselect = false
    };

    private async Task RefreshDashboardDataAsync()
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

    private sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }
}
