using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DnDCharacterManager.Services.Data;
using DnDCharacterManager.Services.Data.Repositories;
using DnDCharacterManager.Services.Import;
using DnDCharacterManager.Services.Navigation;
using DnDCharacterManager.Services.Rules;
using DnDCharacterManager.Services.Rules.Dice;
using DnDCharacterManager.ViewModels.Pages;
using DnDCharacterManager.Views;

namespace DnDCharacterManager;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddDbContext<AppDbContext>();

                services.AddScoped<ICharacterRepository, CharacterRepository>();
                services.AddScoped<IEnemyRepository, EnemyRepository>();
                services.AddScoped<IEquipmentRepository, EquipmentRepository>();
                services.AddScoped<IEncounterRepository, EncounterRepository>();

                services.AddSingleton<IRuleEngine, Dnd5eRuleEngine>();
                services.AddSingleton<IDiceService, DiceService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddScoped<IRulebookImporter, RulebookImporter>();

                services.AddTransient<DashboardViewModel>();
                services.AddTransient<CharactersViewModel>();
                services.AddTransient<CharacterCreationViewModel>();
                services.AddTransient<CharacterDetailViewModel>();
                services.AddTransient<EnemiesViewModel>();
                services.AddTransient<EquipmentViewModel>();
                services.AddTransient<CombatViewModel>();
                services.AddTransient<DiceRollerViewModel>();
                services.AddTransient<SettingsViewModel>();

                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            await _host.StartAsync();

            await using (var scope = _host.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.EnsureCreatedAsync();

                var equipment = scope.ServiceProvider.GetRequiredService<IEquipmentRepository>();
                await equipment.SeedCoreEquipmentAsync();
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The application could not start.\n\n{ex.Message}",
                "D&D Character Manager - Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _host.StopAsync(shutdownTimeout.Token);
        }
        finally
        {
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
