using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Integration.CareerApi.Repositories;

internal sealed class ProjectRepository(ICareerApiClient client) : IProjectRepository
{
    public async Task<IReadOnlyList<CareerProjectDto>> ListAsync(CancellationToken ct)
    {
        var dtos = await client.GetProjectsAsync(null, ct);
        return dtos
            .OrderByDescending(p => p.Start)
            .ToList();
    }

    public Task<CareerProjectDto?> GetAsync(Guid id, CancellationToken ct) =>
        client.GetProjectAsync(id, ct);
}
