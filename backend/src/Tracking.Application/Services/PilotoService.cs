using Tracking.Application.Dtos;
using Tracking.Application.Interfaces;
using Tracking.Domain.Exceptions;

namespace Tracking.Application.Services;

public interface IPilotoService
{
    /// <exception cref="PilotoNoEncontradoException"/>
    Task<IReadOnlyList<PaqueteResumen>> ListarPaquetesAsync(int pilotoId, CancellationToken ct);
}

public class PilotoService(IPilotoRepository pilotos, IPaqueteRepository paquetes) : IPilotoService
{
    public async Task<IReadOnlyList<PaqueteResumen>> ListarPaquetesAsync(int pilotoId, CancellationToken ct)
    {
        if (!await pilotos.ExisteAsync(pilotoId, ct))
            throw new PilotoNoEncontradoException(pilotoId);

        var lista = await paquetes.ListarPorPilotoAsync(pilotoId, ct);
        return lista
            .Select(p => new PaqueteResumen { Guia = p.Guia, Cliente = p.Cliente, Zona = p.Zona, Estado = p.Estado })
            .ToList();
    }
}
