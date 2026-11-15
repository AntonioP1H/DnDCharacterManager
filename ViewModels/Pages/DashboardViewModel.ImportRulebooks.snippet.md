Add the following `using` directives to `DashboardViewModel.cs`:

```csharp
using DnDCharacterManager.Services.Import;
using Microsoft.Maui.Storage;
```

Add a field to `DashboardViewModel`:

```csharp
private readonly IRulebookImporter _rulebookImporter;
```

Add `IRulebookImporter rulebookImporter` to the existing constructor parameters and assign it:

```csharp
_rulebookImporter = rulebookImporter ?? throw new ArgumentNullException(nameof(rulebookImporter));
```

Merge this command into the existing `DashboardViewModel` class. It uses the MAUI file picker instead of hard-coded machine-specific paths:

```csharp
[RelayCommand]
private async Task ImportRulebooksAsync()
{
    if (IsBusy)
        return;

    try
    {
        var basicRules = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select the 5e Basic Rules PDF"
        });
        if (basicRules is null)
            return;

        var monsterManual = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select the 5e Monster Manual PDF"
        });
        if (monsterManual is null)
            return;

        var legacyPhb = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select the 2e Player's Handbook PDF (optional; Cancel to skip)"
        });

        IsBusy = true;
        StatusMessage = "Starting rulebook import...";
        IProgress<string> progress = new Progress<string>(message => StatusMessage = message);

        await _rulebookImporter.ImportBasicRulesAsync(basicRules.FullPath, progress);
        await _rulebookImporter.ImportMonsterManualAsync(monsterManual.FullPath, progress);

        if (legacyPhb is not null)
            await _rulebookImporter.ImportPhb2eAsync(legacyPhb.FullPath, progress);

        await RefreshAsync();
        StatusMessage = "Rulebook import finished.";
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
```

This assumes your existing view model already exposes `IsBusy`, `StatusMessage`, and `RefreshAsync()`, as in the supplied snippet. Keep the command inside the existing class; do not create a second `DashboardViewModel` declaration.
```
