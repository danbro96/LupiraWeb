using LupiraWeb.Server.Domain;
using Marten;

namespace LupiraWeb.Server.Data.Repositories;

internal sealed class ProjectRepository(IQuerySession session) : IProjectRepository
{
    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct) =>
        await session.Query<Project>()
            .OrderByDescending(p => p.Start)
            .ToListAsync(ct);

    public Task<Project?> GetAsync(Guid id, CancellationToken ct) =>
        session.LoadAsync<Project>(id, ct);
}
