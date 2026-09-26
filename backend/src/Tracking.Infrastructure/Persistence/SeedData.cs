using Microsoft.EntityFrameworkCore;
using Tracking.Domain.Entities;

namespace Tracking.Infrastructure.Persistence;

/// <summary>
/// Datos de prueba del contrato. Fechas fijas para que la migración sea determinística.
/// GUA-99999 no se incluye a propósito: es la guía para probar el 404.
/// </summary>
internal static class SeedData
{
    private static readonly DateTime Asignacion = new(2026, 9, 26, 14, 0, 0, DateTimeKind.Utc);

    public static void Aplicar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Piloto>().HasData(
            new Piloto { Id = 1, Codigo = "PIL-001", Nombre = "Carlos Méndez", Activo = true },
            new Piloto { Id = 2, Codigo = "PIL-002", Nombre = "Ana López", Activo = true });

        modelBuilder.Entity<Paquete>().HasData(
            Paquete(1, "GUA-10001", "Ferretería El Constructor", "5555-1234", "12 Calle 4-50 Zona 10", "Zona 10", EstadoPaquete.PENDIENTE, 1),
            Paquete(2, "GUA-10002", "Farmacia La Salud", "5555-2345", "6a Avenida 8-20 Zona 1", "Zona 1", EstadoPaquete.EN_RUTA, 1),
            Paquete(3, "GUA-10003", "Librería Cervantes", "5555-3456", "Calzada Roosevelt 22-43 Zona 11", "Zona 11", EstadoPaquete.ENTREGADO, 1),
            Paquete(4, "GUA-10004", "Supermercado Don Pedro", "5555-4567", "Boulevard Los Próceres 18-60 Zona 10", "Zona 10", EstadoPaquete.NO_ENTREGADO, 1,
                "Cliente ausente, se dejó aviso"),
            Paquete(5, "GUA-10005", "Panadería San Martín", "5555-5678", "Avenida Petapa 45-12 Zona 12", "Zona 12", EstadoPaquete.PENDIENTE, 2),
            Paquete(6, "GUA-10006", "Óptica Visión Clara", null, "7a Avenida 3-33 Zona 9", "Zona 9", EstadoPaquete.EN_RUTA, 2),
            Paquete(7, "GUA-10007", "Taller Mecánico Rápido", "5555-7890", "Calzada Aguilar Batres 34-70 Zona 11", "Zona 11", EstadoPaquete.PENDIENTE, 2),
            Paquete(8, "GUA-10008", "Tienda Deportiva Campeón", "5555-8901", "20 Calle 10-15 Zona 15", "Zona 15", EstadoPaquete.ENTREGADO, 2));
    }

    private static Paquete Paquete(int id, string guia, string cliente, string? telefono, string direccion, string zona,
        EstadoPaquete estado, int pilotoId, string? observaciones = null) => new()
    {
        Id = id,
        Guia = guia,
        Cliente = cliente,
        Telefono = telefono,
        Direccion = direccion,
        Zona = zona,
        Estado = estado,
        Observaciones = observaciones,
        PilotoId = pilotoId,
        FechaAsignacion = Asignacion,
        UltimaActualizacion = Asignacion
    };
}
