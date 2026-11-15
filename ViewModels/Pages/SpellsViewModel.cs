using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DnDCharacterManager.Models.Equipment;
using DnDCharacterManager.Services.Data.Repositories;

namespace DnDCharacterManager.ViewModels.Pages;

/// <summary>
/// Loads and filters spells for a spell-selection or spell-management page.
/// </summary>
public sealed class SpellsViewModel : ObservableObject
{
    private readonly ISpellRepository _spellRepository;
    private bool _isLoading;
    private string? _errorMessage;
    private int? _selectedLevel;
    private string? _selectedClass;
    private string? _selectedSchool;

    public SpellsViewModel(ISpellRepository spellRepository)
    {
        _spellRepository = spellRepository;
    }

    public ObservableCollection<Spell> Spells { get; } = new();

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

    public int? SelectedLevel
    {
        get => _selectedLevel;
        set => SetProperty(ref _selectedLevel, value);
    }

    public string? SelectedClass
    {
        get => _selectedClass;
        set => SetProperty(ref _selectedClass, value);
    }

    public string? SelectedSchool
    {
        get => _selectedSchool;
        set => SetProperty(ref _selectedSchool, value);
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            IReadOnlyList<Spell> spells;

            if (!string.IsNullOrWhiteSpace(SelectedClass))
            {
                spells = await _spellRepository.GetByClassAsync(SelectedClass);
            }
            else if (!string.IsNullOrWhiteSpace(SelectedSchool))
            {
                spells = await _spellRepository.GetBySchoolAsync(SelectedSchool);
            }
            else if (SelectedLevel.HasValue)
            {
                spells = await _spellRepository.GetByLevelAsync(SelectedLevel.Value);
            }
            else
            {
                spells = await _spellRepository.GetAllAsync();
            }

            Spells.Clear();
            foreach (var spell in spells)
            {
                // Apply any additional selected filters together.
                if (SelectedLevel.HasValue && spell.Level != SelectedLevel.Value)
                    continue;

                if (!string.IsNullOrWhiteSpace(SelectedSchool) &&
                    !string.Equals(spell.School, SelectedSchool, StringComparison.OrdinalIgnoreCase))
                    continue;

                Spells.Add(spell);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load spells: {ex.Message}";
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
