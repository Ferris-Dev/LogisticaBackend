using Tracking.Domain.Entities;

namespace Tracking.Application.Interfaces;

public interface IPilotoRepository
{
    Task<bool> ExisteAsync(int pilotoId, CancellationToken ct);
}

public interface IPaqueteRepository
{
    /// <summary>Paquete con seguimiento de cambios, listo para modificar.</summary>
    Task<Paquete?> ObtenerPorGuiaAsync(string guia, CancellationToken ct);

    Task<IReadOnlyList<Paquete>> ListarPorPilotoAsync(int pilotoId, CancellationToken ct);

    Task GuardarCambiosAsync(CancellationToken ct);
}
