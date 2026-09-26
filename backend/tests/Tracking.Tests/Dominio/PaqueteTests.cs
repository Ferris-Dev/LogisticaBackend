using Tracking.Domain.Entities;

namespace Tracking.Tests.Dominio;

public class PaqueteTests
{
    private static readonly DateTime Asignacion = new(2026, 9, 26, 14, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Ahora = new(2026, 9, 26, 18, 0, 0, DateTimeKind.Utc);

    private static Paquete NuevoPaquete() => new()
    {
        Guia = "GUA-10001",
        Cliente = "Cliente",
        Direccion = "Dirección",
        Zona = "Zona 1",
        Estado = EstadoPaquete.PENDIENTE,
        FechaAsignacion = Asignacion,
        UltimaActualizacion = Asignacion
    };

    [Fact]
    public void MismaFechaQueUltimaActualizacion_SeAplica()
    {
        var paquete = NuevoPaquete();

        var historial = paquete.CambiarEstado(EstadoPaquete.EN_RUTA, Asignacion, null, Ahora, "APP");

        Assert.NotNull(historial);
        Assert.Equal(EstadoPaquete.EN_RUTA, paquete.Estado);
    }

    [Fact]
    public void SoloObservaciones_SeAplicaYRegistraHistorial()
    {
        var paquete = NuevoPaquete();

        var historial = paquete.CambiarEstado(EstadoPaquete.PENDIENTE, Ahora, "Llamar antes", Ahora, "APP");

        Assert.NotNull(historial);
        Assert.Equal("Llamar antes", paquete.Observaciones);
    }

    [Fact]
    public void ObservacionesVacias_BorranLasExistentes()
    {
        var paquete = NuevoPaquete();
        paquete.Observaciones = "Anterior";

        paquete.CambiarEstado(EstadoPaquete.EN_RUTA, Ahora, "", Ahora, "APP");

        Assert.Null(paquete.Observaciones);
    }

    [Fact]
    public void ObservacionesNull_NoModificanLasExistentes()
    {
        var paquete = NuevoPaquete();
        paquete.Observaciones = "Anterior";

        paquete.CambiarEstado(EstadoPaquete.EN_RUTA, Ahora, null, Ahora, "APP");

        Assert.Equal("Anterior", paquete.Observaciones);
    }
}
