using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tracking.Application.Interfaces;
using Tracking.Infrastructure.Persistence;
using Tracking.Infrastructure.Repositories;

namespace Tracking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Comportamiento moderno de Npgsql: DateTime Utc ↔ timestamptz.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

        var connectionString = DatabaseConfig.ObtenerConnectionString(config);

        services.AddDbContext<TrackingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IPaqueteRepository, PaqueteRepository>();
        services.AddScoped<IPilotoRepository, PilotoRepository>();
        services.AddHealthChecks().AddNpgSql(connectionString, name: "postgres");

        return services;
    }

    /// <summary>
    /// Aplica migraciones pendientes con 5 reintentos y espera incremental
    /// (en Railway la BD puede tardar en aceptar conexiones).
    /// </summary>
    public static async Task MigrarBaseDeDatosAsync(this IServiceProvider services, ILogger logger)
    {
        const int intentos = 5;
        for (var intento = 1; ; intento++)
        {
            try
            {
                using var scope = services.CreateScope();
                await scope.ServiceProvider.GetRequiredService<TrackingDbContext>().Database.MigrateAsync();
                logger.LogInformation("Migraciones aplicadas");
                return;
            }
            catch (Exception ex) when (intento < intentos)
            {
                var espera = TimeSpan.FromSeconds(2 * intento);
                logger.LogWarning(ex, "No se pudo migrar la BD (intento {Intento}/{Total}); reintento en {Espera}s",
                    intento, intentos, espera.TotalSeconds);
                await Task.Delay(espera);
            }
        }
    }
}
