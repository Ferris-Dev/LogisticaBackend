using Microsoft.EntityFrameworkCore;
using Tracking.Application.Interfaces;
using Tracking.Infrastructure.Persistence;

namespace Tracking.Infrastructure.Repositories;

public class PilotoRepository(TrackingDbContext db) : IPilotoRepository
{
    public Task<bool> ExisteAsync(int pilotoId, CancellationToken ct) =>
        db.Pilotos.AnyAsync(p => p.Id == pilotoId, ct);
}
