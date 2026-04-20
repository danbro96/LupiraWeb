using LupiraWeb.Admin.Server.Endpoints.Artifacts.Dtos;
using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Admin.Server.Endpoints.Artifacts;

public class ArtifactsHandler(IDocumentSession session)
{
    public async Task<Ok<RegisterArtifactResponse>> RegisterAsync(
        RegisterArtifactRequest request, CancellationToken ct)
    {
        var artifactId = Guid.CreateVersion7();
        session.Events.StartStream<Artifact>(artifactId,
            new ArtifactRegistered(
                artifactId,
                request.Kind,
                request.Url,
                request.Title,
                request.Description,
                request.ProducedOn,
                DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return TypedResults.Ok(new RegisterArtifactResponse { ArtifactId = artifactId });
    }

    public async Task<Results<NoContent, NotFound>> LinkToProjectAsync(
        Guid artifactId, LinkArtifactToProjectRequest request, CancellationToken ct)
    {
        var artifact = await session.LoadAsync<Artifact>(artifactId, ct);
        if (artifact is null) return TypedResults.NotFound();

        session.Events.Append(artifactId,
            new ArtifactLinkedToProject(artifactId, request.ProjectId, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound>> LinkToEngagementAsync(
        Guid artifactId, LinkArtifactToEngagementRequest request, CancellationToken ct)
    {
        var artifact = await session.LoadAsync<Artifact>(artifactId, ct);
        if (artifact is null) return TypedResults.NotFound();

        session.Events.Append(artifactId,
            new ArtifactLinkedToEngagement(artifactId, request.EngagementId, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound>> LinkToSkillAsync(
        Guid artifactId, LinkArtifactToSkillRequest request, CancellationToken ct)
    {
        var artifact = await session.LoadAsync<Artifact>(artifactId, ct);
        if (artifact is null) return TypedResults.NotFound();

        session.Events.Append(artifactId,
            new ArtifactLinkedToSkill(artifactId, request.SkillId, request.Role, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}
