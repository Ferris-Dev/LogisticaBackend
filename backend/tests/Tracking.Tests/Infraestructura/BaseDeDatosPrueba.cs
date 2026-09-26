using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracking.Infrastructure.Persistence;

namespace Tracking.Tests.Infraestructura;

/// <summary>
/// SQLite en memoria con el mismo modelo y seed que Postgres.
/// La conexión debe permanecer abierta: al cerrarla la BD desaparece.
/// </summary>
public sealed class BaseDeDatosPrueba : IDisposable
{
    public SqliteConnection Conexion { get; } = new("DataSource=:memory:");

    public BaseDeDatosPrueba()
    {
        Conexion.Open();
        using var db = CrearContexto();
        db.Database.EnsureCreated();
    }

    public TrackingDbContext CrearContexto() =>
        new(new DbContextOptionsBuilder<TrackingDbContext>().UseSqlite(Conexion).Options);

    public void Dispose() => Conexion.Dispose();
}

public sealed class RelojFijo(DateTime ahoraUtc) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(ahoraUtc, TimeSpan.Zero);
}
