using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Services.Data.Repositories;

namespace DnDCharacterManager.ViewModels.Pages;

/// <summary>
/// Loads backgrounds for a background-selection or background-management page.
/// </summary>
public sealed class BackgroundsViewModel : ObservableObject
{
    private readonly IBackgroundRepository _backgroundRepository;
    private bool _isLoading;
    private string? _errorMessage;

    public BackgroundsViewModel(IBackgroundRepository backgroundRepository)
    {
        _backgroundRepository = backgroundRepository;
    }

    public ObservableCollection<Background> Backgrounds { get; } = new();

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
            var backgrounds = await _backgroundRepository.GetAllAsync();
            Backgrounds.Clear();

            foreach (var background in backgrounds)
            {
                Backgrounds.Add(background);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load backgrounds: {ex.Message}";
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
