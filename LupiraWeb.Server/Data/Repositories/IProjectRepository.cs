using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Data.Repositories;

public interface IProjectRepository
{
    Task<IReadOnlyList<CareerProjectDto>> ListAsync(CancellationToken ct);
    Task<CareerProjectDto?> GetAsync(Guid id, CancellationToken ct);
}
