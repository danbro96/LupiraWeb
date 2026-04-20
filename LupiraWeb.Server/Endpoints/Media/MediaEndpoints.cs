using LupiraWeb.Server.Endpoints.Media.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LupiraWeb.Server.Endpoints.Media;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media").WithTags("Media");

        group.MapGet("/",
                (MediaHandler handler, CancellationToken ct) => handler.ListAsync(ct))
            .WithName("ListMedia");

        group.MapGet("/{id:guid}",
                (Guid id, MediaHandler handler, CancellationToken ct) => handler.GetAsync(id, ct))
            .WithName("GetMedia");

        group.MapGet("/{id:guid}/blob",
                (Guid id, MediaHandler handler, CancellationToken ct) => handler.DownloadAsync(id, ct))
            .WithName("DownloadMedia");

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
