using LupiraWeb.Server.Domain;
using Marten;

namespace LupiraWeb.Server.Data.Repositories;

internal sealed class EngagementRepository(IQuerySession session) : IEngagementRepository
{
    public async Task<IReadOnlyList<Engagement>> ListAsync(CancellationToken ct) =>
        await session.Query<Engagement>()
            .OrderByDescending(e => e.Start)
            .ToListAsync(ct);

    public Task<Engagement?> GetAsync(Guid id, CancellationToken ct) =>
        session.LoadAsync<Engagement>(id, ct);
}
