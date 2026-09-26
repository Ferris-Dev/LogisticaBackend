using Microsoft.EntityFrameworkCore;
using Tracking.Application.Dtos;
using Tracking.Application.Exceptions;
using Tracking.Application.Services;
using Tracking.Domain.Entities;
using Tracking.Domain.Exceptions;
using Tracking.Infrastructure.Repositories;
using Tracking.Tests.Infraestructura;

namespace Tracking.Tests.Servicios;

public sealed class TrackingServiceTests : IDisposable
{
    private static readonly DateTime Ahora = new(2026, 9, 26, 18, 0, 0, DateTimeKind.Utc);

    private readonly BaseDeDatosPrueba _bd = new();

    private TrackingService CrearServicio() =>
        new(new PaqueteRepository(_bd.CrearContexto()), new RelojFijo(Ahora));

    public void Dispose() => _bd.Dispose();

    [Fact]
    public async Task Obtener_GuiaExistente_DevuelveDetalle()
    {
        var detalle = await CrearServicio().ObtenerAsync("GUA-10001", CancellationToken.None);

        Assert.Equal("Ferretería El Constructor", detalle.Cliente);
        Assert.Equal(EstadoPaquete.PENDIENTE, detalle.Estado);
        Assert.Equal(DateTimeKind.Utc, detalle.FechaAsignacion.Kind);
    }

    [Fact]
    public async Task Obtener_GuiaEnMinusculas_SeNormaliza()
    {
        var detalle = await CrearServicio().ObtenerAsync("  gua-10001 ", CancellationToken.None);

        Assert.Equal("GUA-10001", detalle.Guia);
    }

    [Fact]
    public async Task Obtener_GuiaInexistente_LanzaGuiaNoEncontrada()
    {
        var ex = await Assert.ThrowsAsync<GuiaNoEncontradaException>(
            () => CrearServicio().ObtenerAsync("GUA-99999", CancellationToken.None));

        Assert.Equal("La guía GUA-99999 no existe", ex.Message);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("10001")]
    [InlineData("GUA-1234")]
    [InlineData("GUA-123456")]
    [InlineData("")]
    public async Task Obtener_FormatoInvalido_LanzaFormatoGuiaInvalido(string guia)
    {
        await Assert.ThrowsAsync<FormatoGuiaInvalidoException>(
            () => CrearServicio().ObtenerAsync(guia, CancellationToken.None));
    }

    [Fact]
    public async Task CambiarEstado_Valido_CambiaEstadoYRegistraHistorial()
    {
        var request = new CambioEstadoRequest { Estado = "ENTREGADO", FechaCambio = "2026-09-26T16:00:00Z" };

        var detalle = await CrearServicio().CambiarEstadoAsync("GUA-10001", request, CancellationToken.None);

        Assert.Equal(EstadoPaquete.ENTREGADO, detalle.Estado);
        Assert.Equal(new DateTime(2026, 9, 26, 16, 0, 0, DateTimeKind.Utc), detalle.UltimaActualizacion);

        await using var db = _bd.CrearContexto();
        var guardado = await db.Paquetes.Include(p => p.Historial).SingleAsync(p => p.Guia == "GUA-10001");
        Assert.Equal(EstadoPaquete.ENTREGADO, guardado.Estado);
        var historial = Assert.Single(guardado.Historial);
        Assert.Equal(EstadoPaquete.PENDIENTE, historial.EstadoAnterior);
        Assert.Equal(EstadoPaquete.ENTREGADO, historial.EstadoNuevo);
        Assert.Equal(new DateTime(2026, 9, 26, 16, 0, 0, DateTimeKind.Utc), historial.FechaCambio);
        Assert.Equal(Ahora, historial.FechaRegistro);
        Assert.Equal("APP", historial.Origen);
    }

    [Fact]
    public async Task CambiarEstado_FechaAnteriorAUltimaActualizacion_NoSobrescribe()
    {
        await CrearServicio().CambiarEstadoAsync("GUA-10001",
            new CambioEstadoRequest { Estado = "ENTREGADO", FechaCambio = "2026-09-26T16:00:00Z" }, CancellationToken.None);

        // La app offline reenvía un cambio más viejo.
        var detalle = await CrearServicio().CambiarEstadoAsync("GUA-10001",
            new CambioEstadoRequest { Estado = "EN_RUTA", FechaCambio = "2026-09-26T15:00:00Z" }, CancellationToken.None);

        Assert.Equal(EstadoPaquete.ENTREGADO, detalle.Estado);
        Assert.Equal(new DateTime(2026, 9, 26, 16, 0, 0, DateTimeKind.Utc), detalle.UltimaActualizacion);

        await using var db = _bd.CrearContexto();
        Assert.Equal(1, await db.HistorialEstados.CountAsync());
    }

    [Fact]
    public async Task CambiarEstado_ReenvioIdentico_NoDuplicaHistorial()
    {
        var request = new CambioEstadoRequest { Estado = "ENTREGADO", FechaCambio = "2026-09-26T16:00:00Z" };

        await CrearServicio().CambiarEstadoAsync("GUA-10001", request, CancellationToken.None);
        await CrearServicio().CambiarEstadoAsync("GUA-10001", request, CancellationToken.None);

        await using var db = _bd.CrearContexto();
        Assert.Equal(1, await db.HistorialEstados.CountAsync());
    }

    [Fact]
    public async Task CambiarEstado_SinFecha_UsaHoraDelServidor()
    {
        var detalle = await CrearServicio().CambiarEstadoAsync("GUA-10001",
            new CambioEstadoRequest { Estado = "EN_RUTA" }, CancellationToken.None);

        Assert.Equal(Ahora, detalle.UltimaActualizacion);
    }

    [Fact]
    public async Task CambiarEstado_FechaConZonaHoraria_SeConvierteAUtc()
    {
        var detalle = await CrearServicio().CambiarEstadoAsync("GUA-10001",
            new CambioEstadoRequest { Estado = "EN_RUTA", FechaCambio = "2026-09-26T10:30:00-06:00" }, CancellationToken.None);

        Assert.Equal(new DateTime(2026, 9, 26, 16, 30, 0, DateTimeKind.Utc), detalle.UltimaActualizacion);
    }

    [Theory]
    [InlineData("PERDIDO")]
    [InlineData("entregado")]
    [InlineData("1")]
    public async Task CambiarEstado_EstadoNoPermitido_LanzaValidacion(string estado)
    {
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => CrearServicio().CambiarEstadoAsync("GUA-10001",
            new CambioEstadoRequest { Estado = estado }, CancellationToken.None));

        Assert.Equal(["Valor no permitido"], ex.Errores["estado"]);
    }

    [Theory]
    [InlineData("ayer")]
    [InlineData("2026-09-26T18:10:00Z")] // 10 min en el futuro: supera la tolerancia
    public async Task CambiarEstado_FechaInvalida_LanzaValidacion(string fecha)
    {
        var ex = await Assert.ThrowsAsync<ValidacionException>(() => CrearServicio().CambiarEstadoAsync("GUA-10001",
            new CambioEstadoRequest { Estado = "ENTREGADO", FechaCambio = fecha }, CancellationToken.None));

        Assert.True(ex.Errores.ContainsKey("fechaCambio"));
    }

    [Fact]
    public async Task CambiarEstado_GuiaInexistente_LanzaGuiaNoEncontrada()
    {
        await Assert.ThrowsAsync<GuiaNoEncontradaException>(() => CrearServicio().CambiarEstadoAsync("GUA-99999",
            new CambioEstadoRequest { Estado = "ENTREGADO" }, CancellationToken.None));
    }
}
