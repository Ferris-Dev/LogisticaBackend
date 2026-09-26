using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Tracking.Infrastructure;

public static class DatabaseConfig
{
    /// <summary>
    /// 1. DATABASE_URL (URI postgresql://user:pass@host:port/db que inyecta Railway).
    /// 2. ConnectionStrings:DefaultConnection (appsettings.Development.json / user-secrets).
    /// </summary>
    public static string ObtenerConnectionString(IConfiguration config)
    {
        var url = config["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(url))
            return DesdeUrl(url);

        var cadena = config.GetConnectionString("DefaultConnection");
        return !string.IsNullOrWhiteSpace(cadena)
            ? cadena
            : throw new InvalidOperationException(
                "No hay cadena de conexión: define DATABASE_URL o ConnectionStrings:DefaultConnection.");
    }

    /// <summary>Npgsql no acepta el formato URI, así que se convierte a "clave=valor".</summary>
    public static string DesdeUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("postgres" or "postgresql"))
            throw new InvalidOperationException("DATABASE_URL debe tener el formato postgresql://usuario:clave@host:puerto/basedatos");

        var credenciales = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credenciales[0]),
            Password = credenciales.Length > 1 ? Uri.UnescapeDataString(credenciales[1]) : null,
            SslMode = SslMode.Prefer
        };

        return builder.ConnectionString + ";Trust Server Certificate=true";
    }
}
