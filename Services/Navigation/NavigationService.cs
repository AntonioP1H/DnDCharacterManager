 using System.Windows.Controls;

  namespace DnDCharacterManager.Services.Navigation;

  public interface INavigationService
  {
      void NavigateTo(string pageKey);
      void NavigateTo<T>() where T : Page;
      void GoBack();
      bool CanGoBack { get; }
      event Action<Page>? Navigated;
  }

  public class NavigationService : INavigationService
  {
      private Frame? _frame;
      private readonly Dictionary<string, Type> _pageMap = new();
      private readonly Stack<Page> _history = new();

      public bool CanGoBack => _history.Count > 0;
      public event Action<Page>? Navigated;

      public void SetFrame(Frame frame)
      {
          _frame = frame;
          _frame.Navigated += (s, e) => Navigated?.Invoke(e.Content as Page);
      }

      public void RegisterPage(string key, Type pageType)
      {
          if (!typeof(Page).IsAssignableFrom(pageType))
              throw new ArgumentException($"Type {pageType.Name} must inherit from Page");
          _pageMap[key] = pageType;
      }

      public void NavigateTo(string pageKey)
      {
          if (!_pageMap.TryGetValue(pageKey, out var pageType))
              throw new ArgumentException($"Page not registered: {pageKey}");

          if (_frame?.Content is Page currentPage)
              _history.Push(currentPage);

          _frame?.Navigate(Activator.CreateInstance(pageType) as Page);
      }

      public void NavigateTo<T>() where T : Page
      {
          var key = typeof(T).Name.Replace("Page", "");
          NavigateTo(key);
      }

      public void GoBack()
      {
          if (_history.Count > 0 && _frame != null)
          {
              var previousPage = _history.Pop();
              _frame.Content = previousPage;
          }
      }
  }