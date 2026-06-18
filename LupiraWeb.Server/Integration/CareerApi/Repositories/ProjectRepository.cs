using LupiraWeb.Domain;
using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Integration.CareerApi.Repositories;

internal sealed class ProjectRepository(ICareerApiClient client) : IProjectRepository
{
    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct)
    {
        var dtos = await client.GetProjectsAsync(null, ct);
        return dtos
            .OrderByDescending(p => p.Start)
            .Select(Map)
            .ToList();
    }

    public async Task<Project?> GetAsync(Guid id, CancellationToken ct)
    {
        var dto = await client.GetProjectAsync(id, ct);
        return dto is null ? null : Map(dto);
    }

    private static Project Map(CareerProjectDto p) => new()
    {
        Id = p.Id,
        Kind = p.Kind,
        Title = p.Title,
        Description = p.Description,
        Url = p.Url,
        EngagementId = p.EngagementId,
        Start = p.Start,
        End = p.End,
        Outcome = p.Outcome,
        Status = p.Status,
        SkillIds = p.SkillIds.ToList(),
    };
}
