using LupiraWeb.Admin.Server.Endpoints.Artifacts.Dtos;

namespace LupiraWeb.Admin.Server.Endpoints.Artifacts;

public static class ArtifactsEndpoints
{
    public static IEndpointRouteBuilder MapArtifactsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/artifacts").WithTags("Artifacts");

        group.MapPost("/",
                (RegisterArtifactRequest request, ArtifactsHandler handler, CancellationToken ct) =>
                    handler.RegisterAsync(request, ct))
            .WithName("RegisterArtifact");

        group.MapPost("/{id:guid}/links/project",
                (Guid id, LinkArtifactToProjectRequest request,
                 ArtifactsHandler handler, CancellationToken ct) =>
                    handler.LinkToProjectAsync(id, request, ct))
            .WithName("LinkArtifactToProject");

        group.MapPost("/{id:guid}/links/engagement",
                (Guid id, LinkArtifactToEngagementRequest request,
                 ArtifactsHandler handler, CancellationToken ct) =>
                    handler.LinkToEngagementAsync(id, request, ct))
            .WithName("LinkArtifactToEngagement");

        group.MapPost("/{id:guid}/links/skill",
                (Guid id, LinkArtifactToSkillRequest request,
                 ArtifactsHandler handler, CancellationToken ct) =>
                    handler.LinkToSkillAsync(id, request, ct))
            .WithName("LinkArtifactToSkill");

        return app;
    }
}
