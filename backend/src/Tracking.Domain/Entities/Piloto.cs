namespace Tracking.Domain.Entities;

public class Piloto
{
    public int Id { get; set; }
    public required string Codigo { get; set; }
    public required string Nombre { get; set; }
    public bool Activo { get; set; } = true;

    public List<Paquete> Paquetes { get; set; } = [];
}
