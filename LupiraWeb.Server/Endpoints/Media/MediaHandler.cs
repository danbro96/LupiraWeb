using LupiraWeb.Server.Domain;
using LupiraWeb.Server.Endpoints.Media.Dtos;
using LupiraWeb.Server.Infrastructure.BlobStorage;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Media;

public class MediaHandler(IDocumentSession session, IBlobStorage blobStorage)
{
    public async Task<Ok<IReadOnlyList<MediaAssetDto>>> ListAsync(CancellationToken ct)
    {
        var media = await session.Query<MediaAsset>()
            .Where(m => !m.Archived)
            .ToListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<MediaAssetDto>>(
            media.Select(ToDto).ToList());
    }

    public async Task<Results<Ok<MediaAssetDto>, NotFound>> GetAsync(Guid id, CancellationToken ct)
    {
        var media = await session.LoadAsync<MediaAsset>(id, ct);
        if (media is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(ToDto(media));
    }

    public async Task<Results<Ok<MediaUploadResponse>, BadRequest<string>>> UploadAsync(
        IFormFile file,
        string altText,
        string? caption,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return TypedResults.BadRequest("Empty file");

        var mediaId = Guid.CreateVersion7();
        await using var stream = file.OpenReadStream();
        var blobRef = await blobStorage.UploadAsync(stream, file.ContentType, ct);

        session.Events.StartStream<MediaAsset>(mediaId,
            new MediaRegistered(
                mediaId,
                blobRef,
                file.ContentType,
                Width: null,
                Height: null,
                altText,
                caption,
                DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);

        return TypedResults.Ok(new MediaUploadResponse
        {
            MediaId = mediaId,
            BlobRef = blobRef,
        });
    }

    public async Task<Results<NoContent, NotFound>> LinkToProjectAsync(
        Guid mediaId, LinkMediaToProjectRequest request, CancellationToken ct)
    {
        var media = await session.LoadAsync<MediaAsset>(mediaId, ct);
        if (media is null) return TypedResults.NotFound();

        session.Events.Append(mediaId,
            new MediaLinkedToProject(mediaId, request.ProjectId, request.Role, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound>> LinkToSkillsAsync(
        Guid mediaId, LinkMediaToSkillsRequest request, CancellationToken ct)
    {
        var media = await session.LoadAsync<MediaAsset>(mediaId, ct);
        if (media is null) return TypedResults.NotFound();

        var now = DateTimeOffset.UtcNow;
        foreach (var skillId in request.SkillIds.Distinct())
        {
            session.Events.Append(mediaId,
                new MediaLinkedToSkill(mediaId, skillId, request.Note, now));
        }
        await session.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    public async Task<Results<FileStreamHttpResult, NotFound>> DownloadAsync(
        Guid id, CancellationToken ct)
    {
        var media = await session.LoadAsync<MediaAsset>(id, ct);
        if (media is null) return TypedResults.NotFound();

        var blob = await blobStorage.DownloadAsync(media.BlobRef, ct);
        if (blob is null) return TypedResults.NotFound();

        return TypedResults.File(new MemoryStream(blob.Bytes), blob.ContentType);
    }

    private static MediaAssetDto ToDto(MediaAsset m) => new()
    {
        Id = m.Id,
        BlobRef = m.BlobRef,
        MimeType = m.MimeType,
        Width = m.Width,
        Height = m.Height,
        AltText = m.AltText,
        Caption = m.Caption,
        Archived = m.Archived,
        LinkedProjects = m.LinkedProjects.Select(p => new MediaProjectLinkDto
        {
            ProjectId = p.ProjectId,
            Role = p.Role,
        }).ToList(),
        LinkedSkillIds = m.LinkedSkillIds.ToList(),
    };
}
