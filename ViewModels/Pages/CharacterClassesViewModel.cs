using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DnDCharacterManager.Models.Characters;
using DnDCharacterManager.Services.Data.Repositories;

namespace DnDCharacterManager.ViewModels.Pages;

/// <summary>
/// Loads character classes for a class-selection or class-management page.
/// </summary>
public sealed class CharacterClassesViewModel : ObservableObject
{
    private readonly IClassRepository _classRepository;
    private bool _isLoading;
    private string? _errorMessage;

    public CharacterClassesViewModel(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    public ObservableCollection<CharacterClass> CharacterClasses { get; } = new();

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
            var classes = await _classRepository.GetAllAsync();
            CharacterClasses.Clear();

            foreach (var characterClass in classes)
            {
                CharacterClasses.Add(characterClass);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load character classes: {ex.Message}";
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
