using Microsoft.Extensions.Configuration;
using Npgsql;
using Tracking.Infrastructure;

namespace Tracking.Tests.Infraestructura;

public class DatabaseConfigTests
{
    private static IConfiguration Config(params (string Clave, string Valor)[] valores) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(valores.Select(v => new KeyValuePair<string, string?>(v.Clave, v.Valor)))
            .Build();

    [Fact]
    public void DatabaseUrl_EstiloRailway_SeConvierteAFormatoNpgsql()
    {
        var cadena = DatabaseConfig.ObtenerConnectionString(Config(
            ("DATABASE_URL", "postgresql://postgres:p%40ss:word@db.railway.internal:6543/railway"),
            ("ConnectionStrings:DefaultConnection", "Host=ignorado")));

        var builder = new NpgsqlConnectionStringBuilder(cadena);
        Assert.Equal("db.railway.internal", builder.Host);
        Assert.Equal(6543, builder.Port);
        Assert.Equal("railway", builder.Database);
        Assert.Equal("postgres", builder.Username);
        Assert.Equal("p@ss:word", builder.Password);
        Assert.Equal(SslMode.Prefer, builder.SslMode);
    }

    [Fact]
    public void SinDatabaseUrl_UsaDefaultConnection()
    {
        var cadena = DatabaseConfig.ObtenerConnectionString(Config(
            ("ConnectionStrings:DefaultConnection", "Host=localhost;Database=tracking")));

        Assert.Equal("Host=localhost;Database=tracking", cadena);
    }

    [Fact]
    public void SinNingunaCadena_LanzaErrorClaro()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => DatabaseConfig.ObtenerConnectionString(Config()));

        Assert.Contains("DATABASE_URL", ex.Message);
    }
}
