using LupiraWeb.Domain;
using LupiraWeb.Domain.Infrastructure.BlobStorage;
using LupiraWeb.Server.Endpoints.Media.Dtos;
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
