using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Data.Repositories;

public interface IEngagementRepository
{
    Task<IReadOnlyList<CareerEngagementDto>> ListAsync(CancellationToken ct);
    Task<CareerEngagementDto?> GetAsync(Guid id, CancellationToken ct);
}
