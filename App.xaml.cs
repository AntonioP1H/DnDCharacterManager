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

/// <summary>WPF application entry point and dependency-injection composition root.</summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<AppDbContext>();
                services.AddSingleton<IAppDbContextFactory, AppDbContextFactory>();

                services.AddScoped<ICharacterRepository, CharacterRepository>();
                services.AddScoped<IEnemyRepository, EnemyRepository>();
                services.AddScoped<IEquipmentRepository, EquipmentRepository>();
                services.AddScoped<IEncounterRepository, EncounterRepository>();

                services.AddSingleton<IRuleEngine, Dnd5eRuleEngine>();
                services.AddSingleton<IDiceService, DiceService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IRulebookImporter, RulebookImporter>();

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

                var equipmentRepository = scope.ServiceProvider.GetRequiredService<IEquipmentRepository>();
                await equipmentRepository.SeedCoreEquipmentAsync();
            }

            var window = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The application could not start.\n\n{ex.Message}",
                "Local D&D startup error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
