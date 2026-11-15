using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDCharacterManager.ViewModels.Base;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string _title = string.Empty;

    protected virtual Task OnNavigatedToAsync() => Task.CompletedTask;
    protected virtual Task OnNavigatedFromAsync() => Task.CompletedTask;
}
