using System.Globalization;
using Tracking.Application.Dtos;
using Tracking.Application.Exceptions;
using Tracking.Application.Interfaces;
using Tracking.Domain;
using Tracking.Domain.Entities;
using Tracking.Domain.Exceptions;

namespace Tracking.Application.Services;

public interface ITrackingService
{
    /// <exception cref="FormatoGuiaInvalidoException"/>
    /// <exception cref="GuiaNoEncontradaException"/>
    Task<PaqueteDetalle> ObtenerAsync(string guia, CancellationToken ct);

    /// <summary>Aplica el cambio (last-write-wins) y devuelve el estado vigente del paquete.</summary>
    /// <exception cref="FormatoGuiaInvalidoException"/>
    /// <exception cref="ValidacionException"/>
    /// <exception cref="GuiaNoEncontradaException"/>
    Task<PaqueteDetalle> CambiarEstadoAsync(string guia, CambioEstadoRequest request, CancellationToken ct);
}

public class TrackingService(IPaqueteRepository paquetes, TimeProvider reloj) : ITrackingService
{
    public const int MaxObservaciones = 500;

    // Tolerancia para relojes de celular adelantados. Un cambio muy en el futuro
    // bloquearía el paquete para siempre bajo last-write-wins, por eso se rechaza.
    public static readonly TimeSpan ToleranciaFuturo = TimeSpan.FromMinutes(5);

    private static readonly string[] EstadosValidos = Enum.GetNames<EstadoPaquete>();

    public async Task<PaqueteDetalle> ObtenerAsync(string guia, CancellationToken ct)
    {
        var normalizada = Guia.Normalizar(guia);
        var paquete = await paquetes.ObtenerPorGuiaAsync(normalizada, ct)
            ?? throw new GuiaNoEncontradaException(normalizada);

        return PaqueteDetalle.Desde(paquete);
    }

    public async Task<PaqueteDetalle> CambiarEstadoAsync(string guia, CambioEstadoRequest request, CancellationToken ct)
    {
        var normalizada = Guia.Normalizar(guia);
        var ahora = Truncar(reloj.GetUtcNow().UtcDateTime, TimeSpan.TicksPerSecond);
        var (estado, fechaCambio) = Validar(request, ahora);

        var paquete = await paquetes.ObtenerPorGuiaAsync(normalizada, ct)
            ?? throw new GuiaNoEncontradaException(normalizada);

        if (paquete.CambiarEstado(estado, fechaCambio, request.Observaciones, ahora, HistorialEstado.OrigenApp) is not null)
            await paquetes.GuardarCambiosAsync(ct);

        return PaqueteDetalle.Desde(paquete);
    }

    private static (EstadoPaquete Estado, DateTime FechaCambio) Validar(CambioEstadoRequest request, DateTime ahora)
    {
        var errores = new Dictionary<string, string[]>();

        EstadoPaquete estado = default;
        if (string.IsNullOrWhiteSpace(request.Estado))
            errores["estado"] = ["El estado es obligatorio"];
        // Se compara contra los nombres porque Enum.TryParse también aceptaría "1".
        else if (!EstadosValidos.Contains(request.Estado))
            errores["estado"] = ["Valor no permitido"];
        else
            estado = Enum.Parse<EstadoPaquete>(request.Estado);

        var fechaCambio = ahora;
        if (!string.IsNullOrWhiteSpace(request.FechaCambio))
        {
            if (!DateTimeOffset.TryParse(request.FechaCambio, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var fecha))
                errores["fechaCambio"] = ["Debe ser una fecha ISO-8601, p. ej. 2026-09-26T16:00:00Z"];
            else if (fecha.UtcDateTime > ahora + ToleranciaFuturo)
                errores["fechaCambio"] = ["No puede estar en el futuro"];
            else
                fechaCambio = Truncar(fecha.UtcDateTime, TimeSpan.TicksPerMicrosecond);
        }

        if (request.Observaciones is { Length: > MaxObservaciones })
            errores["observaciones"] = [$"Máximo {MaxObservaciones} caracteres"];

        return errores.Count > 0 ? throw new ValidacionException(errores) : (estado, fechaCambio);
    }

    // Postgres guarda microsegundos; truncar evita que la respuesta del PUT
    // difiera de lo que devolverá un GET posterior.
    private static DateTime Truncar(DateTime fecha, long ticks) =>
        new(fecha.Ticks - fecha.Ticks % ticks, DateTimeKind.Utc);
}
