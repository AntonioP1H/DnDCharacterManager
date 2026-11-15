using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Services.Data.Repositories;

namespace DnDCharacterManager.ViewModels.Pages;

/// <summary>
/// Loads races for a race-selection or race-management page.
/// </summary>
public sealed class RacesViewModel : ObservableObject
{
    private readonly IRaceRepository _raceRepository;
    private bool _isLoading;
    private string? _errorMessage;

    public RacesViewModel(IRaceRepository raceRepository)
    {
        _raceRepository = raceRepository;
    }

    public ObservableCollection<Race> Races { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var races = await _raceRepository.GetAllAsync();
            Races.Clear();

            foreach (var race in races)
            {
                Races.Add(race);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load races: {ex.Message}";
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
