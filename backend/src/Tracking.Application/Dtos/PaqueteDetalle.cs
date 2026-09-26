using Tracking.Domain.Entities;

namespace Tracking.Application.Dtos;

/// <summary>Detalle completo de un paquete.</summary>
public record PaqueteDetalle
{
    /// <example>GUA-10001</example>
    public required string Guia { get; init; }

    /// <example>Ferretería El Constructor</example>
    public required string Cliente { get; init; }

    /// <example>5555-1234</example>
    public string? Telefono { get; init; }

    /// <example>12 Calle 4-50 Zona 10</example>
    public required string Direccion { get; init; }

    /// <example>Zona 10</example>
    public required string Zona { get; init; }

    /// <example>PENDIENTE</example>
    public required EstadoPaquete Estado { get; init; }

    /// <example>null</example>
    public string? Observaciones { get; init; }

    /// <example>1</example>
    public required int PilotoId { get; init; }

    /// <summary>Fecha UTC en que se asignó al piloto.</summary>
    /// <example>2026-09-26T14:00:00Z</example>
    public required DateTime FechaAsignacion { get; init; }

    /// <summary>Fecha UTC del último cambio de estado aplicado.</summary>
    /// <example>2026-09-26T14:00:00Z</example>
    public required DateTime UltimaActualizacion { get; init; }

    public static PaqueteDetalle Desde(Paquete p) => new()
    {
        Guia = p.Guia,
        Cliente = p.Cliente,
        Telefono = p.Telefono,
        Direccion = p.Direccion,
        Zona = p.Zona,
        Estado = p.Estado,
        Observaciones = p.Observaciones,
        PilotoId = p.PilotoId,
        FechaAsignacion = p.FechaAsignacion,
        UltimaActualizacion = p.UltimaActualizacion
    };
}
