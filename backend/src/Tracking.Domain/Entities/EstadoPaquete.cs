namespace Tracking.Domain.Entities;

/// <summary>Estados posibles de un paquete.</summary>
/// <remarks>
/// Los nombres de los miembros son exactamente los valores del contrato:
/// se serializan tal cual en JSON y se guardan tal cual en la base de datos.
/// </remarks>
public enum EstadoPaquete
{
    PENDIENTE,
    EN_RUTA,
    ENTREGADO,
    NO_ENTREGADO
}
