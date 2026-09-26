namespace Tracking.Domain.Entities;

public class HistorialEstado
{
    public const string OrigenApp = "APP";

    public int Id { get; set; }
    public int PaqueteId { get; set; }
    public EstadoPaquete EstadoAnterior { get; set; }
    public EstadoPaquete EstadoNuevo { get; set; }

    /// <summary>Momento en que el piloto hizo el cambio (enviado por la app).</summary>
    public DateTime FechaCambio { get; set; }

    /// <summary>Momento en que el servidor lo registró.</summary>
    public DateTime FechaRegistro { get; set; }

    public required string Origen { get; set; }

    public Paquete? Paquete { get; set; }
}
