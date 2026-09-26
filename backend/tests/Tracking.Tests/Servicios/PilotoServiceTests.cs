using Tracking.Application.Services;
using Tracking.Domain.Entities;
using Tracking.Domain.Exceptions;
using Tracking.Infrastructure.Repositories;
using Tracking.Tests.Infraestructura;

namespace Tracking.Tests.Servicios;

public sealed class PilotoServiceTests : IDisposable
{
    private readonly BaseDeDatosPrueba _bd = new();

    private PilotoService CrearServicio()
    {
        var db = _bd.CrearContexto();
        return new PilotoService(new PilotoRepository(db), new PaqueteRepository(db));
    }

    public void Dispose() => _bd.Dispose();

    [Fact]
    public async Task ListarPaquetes_PilotoExistente_DevuelvePaquetesOrdenadosPorGuia()
    {
        var paquetes = await CrearServicio().ListarPaquetesAsync(1, CancellationToken.None);

        Assert.Equal(["GUA-10001", "GUA-10002", "GUA-10003", "GUA-10004"], paquetes.Select(p => p.Guia));
    }

    [Fact]
    public async Task ListarPaquetes_PilotoSinPaquetes_DevuelveListaVacia()
    {
        await using (var db = _bd.CrearContexto())
        {
            db.Pilotos.Add(new Piloto { Id = 3, Codigo = "PIL-003", Nombre = "Sin carga" });
            await db.SaveChangesAsync();
        }

        var paquetes = await CrearServicio().ListarPaquetesAsync(3, CancellationToken.None);

        Assert.Empty(paquetes);
    }

    [Fact]
    public async Task ListarPaquetes_PilotoInexistente_LanzaPilotoNoEncontrado()
    {
        var ex = await Assert.ThrowsAsync<PilotoNoEncontradoException>(
            () => CrearServicio().ListarPaquetesAsync(99, CancellationToken.None));

        Assert.Equal(99, ex.PilotoId);
    }
}
