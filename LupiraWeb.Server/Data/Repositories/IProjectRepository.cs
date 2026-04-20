using LupiraWeb.Server.Data.Entities;

namespace LupiraWeb.Server.Data.Repositories;

public interface IProjectRepository
{
    Task<IReadOnlyList<ProjectEntity>> ListAsync(CancellationToken ct);
    Task<ProjectEntity?> GetAsync(Guid id, CancellationToken ct);
}
