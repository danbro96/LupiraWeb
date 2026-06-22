using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Integration.CareerApi.Repositories;

internal sealed class EngagementRepository(ICareerApiClient client) : IEngagementRepository
{
    public async Task<IReadOnlyList<CareerEngagementDto>> ListAsync(CancellationToken ct)
    {
        var dtos = await client.GetEngagementsAsync(ct);
        return dtos
            .OrderByDescending(e => e.Start)
            .ToList();
    }

    public Task<CareerEngagementDto?> GetAsync(Guid id, CancellationToken ct) =>
        client.GetEngagementAsync(id, ct);
}
