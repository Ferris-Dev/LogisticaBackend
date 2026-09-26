namespace Tracking.Domain.Entities;

public class Paquete
{
    public int Id { get; set; }
    public required string Guia { get; set; }
    public int PilotoId { get; set; }
    public required string Cliente { get; set; }
    public string? Telefono { get; set; }
    public required string Direccion { get; set; }
    public required string Zona { get; set; }
    public EstadoPaquete Estado { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime UltimaActualizacion { get; set; }

    public Piloto? Piloto { get; set; }
    public List<HistorialEstado> Historial { get; set; } = [];

    /// <summary>
    /// Aplica un cambio de estado con la regla last-write-wins: un cambio con
    /// <paramref name="fechaCambio"/> anterior a <see cref="UltimaActualizacion"/>
    /// es obsoleto (p. ej. la app offline reenvía una cola vieja) y se ignora.
    /// Un cambio que no modifica nada también se ignora, lo que hace el reenvío idempotente.
    /// </summary>
    /// <param name="observaciones">null = no tocar; vacío = borrar.</param>
    /// <returns>El registro de historial si el cambio se aplicó; null si se ignoró.</returns>
    public HistorialEstado? CambiarEstado(EstadoPaquete nuevoEstado, DateTime fechaCambio, string? observaciones,
        DateTime ahora, string origen)
    {
        if (fechaCambio < UltimaActualizacion)
            return null;

        var nuevasObservaciones = observaciones is null
            ? Observaciones
            : string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();

        if (nuevoEstado == Estado && nuevasObservaciones == Observaciones)
            return null;

        var historial = new HistorialEstado
        {
            EstadoAnterior = Estado,
            EstadoNuevo = nuevoEstado,
            FechaCambio = fechaCambio,
            FechaRegistro = ahora,
            Origen = origen
        };

        Estado = nuevoEstado;
        Observaciones = nuevasObservaciones;
        UltimaActualizacion = fechaCambio;
        Historial.Add(historial);

        return historial;
    }
}
