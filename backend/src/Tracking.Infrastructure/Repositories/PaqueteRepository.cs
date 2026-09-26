using Microsoft.EntityFrameworkCore;
using Tracking.Application.Interfaces;
using Tracking.Domain.Entities;
using Tracking.Infrastructure.Persistence;

namespace Tracking.Infrastructure.Repositories;

public class PaqueteRepository(TrackingDbContext db) : IPaqueteRepository
{
    public Task<Paquete?> ObtenerPorGuiaAsync(string guia, CancellationToken ct) =>
        db.Paquetes.FirstOrDefaultAsync(p => p.Guia == guia, ct);

    public async Task<IReadOnlyList<Paquete>> ListarPorPilotoAsync(int pilotoId, CancellationToken ct) =>
        await db.Paquetes
            .AsNoTracking()
            .Where(p => p.PilotoId == pilotoId)
            .OrderBy(p => p.Guia)
            .ToListAsync(ct);

    public Task GuardarCambiosAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
