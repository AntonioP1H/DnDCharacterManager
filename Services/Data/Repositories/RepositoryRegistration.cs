using System;
using DnDCharacterManager.Services.Import;
using Microsoft.Extensions.DependencyInjection;

namespace DnDCharacterManager.Services.Data.Repositories;

public static class RepositoryRegistration
{
    /// <summary>
    /// Call after registering AppDbContext in the application's service collection.
    /// </summary>
    public static IServiceCollection AddDndPhase2Services(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IRaceRepository, RaceRepository>();
        services.AddScoped<IBackgroundRepository, BackgroundRepository>();
        services.AddScoped<ISpellRepository, SpellRepository>();
        services.AddTransient<IRulebookImporter, RulebookImporter>();

        return services;
    }
}
