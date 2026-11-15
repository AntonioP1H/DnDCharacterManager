using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace DnDCharacterManager.Services.Navigation;

public interface INavigationService
{
    void SetFrame(Frame frame);
    void RegisterPage(string key, Type pageType);
    void NavigateTo(string pageKey);
    void NavigateTo<TPage>() where TPage : Page;
    void GoBack();
    bool CanGoBack { get; }
    event Action<Page?>? Navigated;
}

/// <summary>
/// Creates each page inside its own DI scope. This keeps scoped DbContext/repositories
/// alive for the lifetime of the page and avoids resolving scoped services from root DI.
/// </summary>
public sealed class NavigationService : INavigationService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Dictionary<string, Type> _pageMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Page, IServiceScope> _pageScopes = new();
    private readonly Stack<Page> _history = new();
    private Frame? _frame;
    private bool _disposed;

    public NavigationService(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    public bool CanGoBack => _history.Count > 0;
    public event Action<Page?>? Navigated;

    public void SetFrame(Frame frame)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _frame = frame ?? throw new ArgumentNullException(nameof(frame));
    }

    public void RegisterPage(string key, Type pageType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(pageType);
        if (!typeof(Page).IsAssignableFrom(pageType))
            throw new ArgumentException($"{pageType.FullName} must derive from WPF Page.", nameof(pageType));
        _pageMap[key] = pageType;
    }

    public void NavigateTo(string pageKey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_frame is null)
            throw new InvalidOperationException("The navigation Frame has not been assigned.");
        if (!_pageMap.TryGetValue(pageKey, out Type? pageType))
            throw new ArgumentException($"No page is registered with key '{pageKey}'.", nameof(pageKey));

        if (_frame.Content is Page current && current.GetType() == pageType)
            return;

        IServiceScope scope = _scopeFactory.CreateScope();
        try
        {
            var page = ActivatorUtilities.CreateInstance(scope.ServiceProvider, pageType) as Page
                ?? throw new InvalidOperationException($"Could not create page {pageType.FullName}.");

            if (_frame.Content is Page previous)
                _history.Push(previous);

            _pageScopes.Add(page, scope);
            _frame.Content = page;
            Navigated?.Invoke(page);
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    public void NavigateTo<TPage>() where TPage : Page
    {
        var entry = _pageMap.FirstOrDefault(pair => pair.Value == typeof(TPage));
        if (entry.Key is null)
            throw new ArgumentException($"Page type {typeof(TPage).Name} is not registered.");
        NavigateTo(entry.Key);
    }

    public void GoBack()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_frame is null || _history.Count == 0)
            return;

        Page? current = _frame.Content as Page;
        Page previous = _history.Pop();
        _frame.Content = previous;
        Navigated?.Invoke(previous);

        if (current is not null && !ReferenceEquals(current, previous))
            DisposePageScope(current);
    }

    private void DisposePageScope(Page page)
    {
        if (_pageScopes.Remove(page, out IServiceScope? scope))
            scope.Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (IServiceScope scope in _pageScopes.Values)
            scope.Dispose();

        _pageScopes.Clear();
        _history.Clear();
        _pageMap.Clear();
        _frame = null;
    }
}
