using Tracking.Domain.Entities;

namespace Tracking.Application.Dtos;

/// <summary>Paquete en el listado de un piloto.</summary>
public record PaqueteResumen
{
    /// <example>GUA-10001</example>
    public required string Guia { get; init; }

    /// <example>Ferretería El Constructor</example>
    public required string Cliente { get; init; }

    /// <example>Zona 10</example>
    public required string Zona { get; init; }

    /// <example>PENDIENTE</example>
    public required EstadoPaquete Estado { get; init; }
}
