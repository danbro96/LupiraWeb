using LupiraWeb.Server.Domain;

namespace LupiraWeb.Server.Data.Repositories;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct);
    Task<Project?> GetAsync(Guid id, CancellationToken ct);
}
