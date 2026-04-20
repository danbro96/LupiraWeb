using LupiraWeb.Domain;
using LupiraWeb.Server.Endpoints.Artifacts.Dtos;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Artifacts;

public class ArtifactsHandler(IDocumentSession session)
{
    public async Task<Ok<IReadOnlyList<ArtifactDto>>> ListAsync(CancellationToken ct)
    {
        var artifacts = await session.Query<Artifact>()
            .Where(a => !a.Archived)
            .ToListAsync(ct);
        return TypedResults.Ok<IReadOnlyList<ArtifactDto>>(
            artifacts.Select(ToDto).ToList());
    }

    public async Task<Results<Ok<ArtifactDto>, NotFound>> GetAsync(Guid id, CancellationToken ct)
    {
        var artifact = await session.LoadAsync<Artifact>(id, ct);
        if (artifact is null) return TypedResults.NotFound();
        return TypedResults.Ok(ToDto(artifact));
    }

    private static ArtifactDto ToDto(Artifact a) => new()
    {
        Id = a.Id,
        Kind = a.Kind,
        Url = a.Url,
        Title = a.Title,
        Description = a.Description,
        ProducedOn = a.ProducedOn,
        Archived = a.Archived,
        LinkedProjectIds = a.LinkedProjectIds.ToList(),
        LinkedEngagementIds = a.LinkedEngagementIds.ToList(),
        LinkedSkills = a.LinkedSkills.Select(l => new ArtifactSkillLinkDto
        {
            SkillId = l.SkillId,
            Role = l.Role,
        }).ToList(),
    };
}
