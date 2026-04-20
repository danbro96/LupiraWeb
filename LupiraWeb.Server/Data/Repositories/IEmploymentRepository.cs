using LupiraWeb.Server.Data.Entities;

namespace LupiraWeb.Server.Data.Repositories;

public interface IEmploymentRepository
{
    Task<IReadOnlyList<EmploymentEntity>> ListAsync(CancellationToken ct);
    Task<EmploymentEntity?> GetAsync(Guid id, CancellationToken ct);
}
