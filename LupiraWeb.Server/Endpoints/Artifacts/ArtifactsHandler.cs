using LupiraWeb.Server.Endpoints.Artifacts.Dtos;
using LupiraWeb.Server.Integration.CareerApi;
using LupiraWeb.Server.Integration.CareerApi.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Artifacts;

public class ArtifactsHandler(ICareerApiClient client)
{
    public async Task<Ok<IReadOnlyList<ArtifactDto>>> ListAsync(CancellationToken ct)
    {
        var artifacts = await client.GetArtifactsAsync(ct);
        return TypedResults.Ok<IReadOnlyList<ArtifactDto>>(
            artifacts.Where(a => !a.Archived).Select(ToDto).ToList());
    }

    public async Task<Results<Ok<ArtifactDto>, NotFound>> GetAsync(Guid id, CancellationToken ct)
    {
        var artifact = await client.GetArtifactAsync(id, ct);
        if (artifact is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(ToDto(artifact));
    }

    private static ArtifactDto ToDto(CareerArtifactDto a) => new()
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
