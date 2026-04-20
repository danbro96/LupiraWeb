using LupiraWeb.Admin.Server.Endpoints.Media.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LupiraWeb.Admin.Server.Endpoints.Media;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media").WithTags("Media");

        group.MapPost("/",
                (IFormFile file, [FromForm] string altText, [FromForm] string? caption,
                 MediaHandler handler, CancellationToken ct) =>
                    handler.UploadAsync(file, altText, caption, ct))
            .WithName("UploadMedia")
            .DisableAntiforgery();

        group.MapPost("/{id:guid}/links/projects",
                (Guid id, LinkMediaToProjectRequest request,
                 MediaHandler handler, CancellationToken ct) =>
                    handler.LinkToProjectAsync(id, request, ct))
            .WithName("LinkMediaToProject");

        group.MapPost("/{id:guid}/links/skills",
                (Guid id, LinkMediaToSkillsRequest request,
                 MediaHandler handler, CancellationToken ct) =>
                    handler.LinkToSkillsAsync(id, request, ct))
            .WithName("LinkMediaToSkills");

        return app;
    }
}
