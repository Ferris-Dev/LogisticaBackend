namespace Tracking.Application.Dtos;

/// <summary>Cambio de estado enviado por la app del piloto.</summary>
/// <remarks>
/// Estado y fecha llegan como string para poder responder un 400 con "errors"
/// por campo, en lugar de un fallo genérico de deserialización.
/// </remarks>
public record CambioEstadoRequest
{
    /// <summary>PENDIENTE | EN_RUTA | ENTREGADO | NO_ENTREGADO</summary>
    /// <example>ENTREGADO</example>
    public string? Estado { get; init; }

    /// <summary>
    /// Momento del cambio en ISO-8601 (idealmente UTC). Opcional: si falta se usa la hora del servidor.
    /// Si es anterior a ultimaActualizacion, el cambio se ignora (last-write-wins).
    /// </summary>
    /// <example>2026-09-26T16:00:00Z</example>
    public string? FechaCambio { get; init; }

    /// <summary>Opcional. null = no modificar; "" = borrar.</summary>
    /// <example>Recibió el encargado de bodega</example>
    public string? Observaciones { get; init; }
}
