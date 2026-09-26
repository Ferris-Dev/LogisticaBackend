using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tracking.Infrastructure.Persistence;
using Tracking.Tests.Infraestructura;

namespace Tracking.Tests.Api;

/// <summary>Levanta la API completa en memoria, cambiando Postgres por SQLite.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly BaseDeDatosPrueba _bd = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=no-usado");
        builder.UseSetting("RUN_MIGRATIONS", "false");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TrackingDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<TrackingDbContext>>();
            services.AddDbContext<TrackingDbContext>(o => o.UseSqlite(_bd.Conexion));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _bd.Dispose();
    }
}
