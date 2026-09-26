using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tracking.Domain.Entities;

namespace Tracking.Infrastructure.Persistence;

public partial class TrackingDbContext(DbContextOptions<TrackingDbContext> options) : DbContext(options)
{
    public DbSet<Piloto> Pilotos => Set<Piloto>();
    public DbSet<Paquete> Paquetes => Set<Paquete>();
    public DbSet<HistorialEstado> HistorialEstados => Set<HistorialEstado>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // timestamptz exige Kind=Utc al escribir; al leer se marca Utc para que
        // System.Text.Json serialice con sufijo "Z".
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Piloto>(e =>
        {
            e.ToTable("pilotos");
            e.Property(p => p.Codigo).HasMaxLength(20);
            e.Property(p => p.Nombre).HasMaxLength(100);
            e.HasIndex(p => p.Codigo).IsUnique();
        });

        modelBuilder.Entity<Paquete>(e =>
        {
            e.ToTable("paquetes");
            e.Property(p => p.Guia).HasMaxLength(9);
            e.HasIndex(p => p.Guia).IsUnique();
            e.Property(p => p.Cliente).HasMaxLength(150);
            e.Property(p => p.Telefono).HasMaxLength(20);
            e.Property(p => p.Direccion).HasMaxLength(250);
            e.Property(p => p.Zona).HasMaxLength(50);
            e.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Observaciones).HasMaxLength(500);
            e.HasOne(p => p.Piloto)
                .WithMany(p => p.Paquetes)
                .HasForeignKey(p => p.PilotoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HistorialEstado>(e =>
        {
            e.ToTable("historial_estados");
            e.Property(h => h.EstadoAnterior).HasConversion<string>().HasMaxLength(20);
            e.Property(h => h.EstadoNuevo).HasConversion<string>().HasMaxLength(20);
            e.Property(h => h.Origen).HasMaxLength(20);
            e.HasOne(h => h.Paquete)
                .WithMany(p => p.Historial)
                .HasForeignKey(h => h.PaqueteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData.Aplicar(modelBuilder);
        AplicarSnakeCase(modelBuilder);
    }

    private static void AplicarSnakeCase(ModelBuilder modelBuilder)
    {
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var propiedad in entidad.GetProperties())
                propiedad.SetColumnName(SnakeCase(propiedad.Name));
            foreach (var clave in entidad.GetKeys())
                clave.SetName(SnakeCase(clave.GetName()!));
            foreach (var fk in entidad.GetForeignKeys())
                fk.SetConstraintName(SnakeCase(fk.GetConstraintName()!));
            foreach (var indice in entidad.GetIndexes())
                indice.SetDatabaseName(SnakeCase(indice.GetDatabaseName()!));
        }
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex LimitePalabra();

    private static string SnakeCase(string nombre) => LimitePalabra().Replace(nombre, "$1_$2").ToLowerInvariant();

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.ToUniversalTime(), DateTimeKind.Utc),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
}
