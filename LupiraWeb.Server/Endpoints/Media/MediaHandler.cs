using LupiraWeb.Server.Endpoints.Media.Dtos;
using LupiraWeb.Server.Integration.CareerApi;
using LupiraWeb.Server.Integration.CareerApi.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Media;

public class MediaHandler(ICareerApiClient client)
{
    public async Task<Ok<IReadOnlyList<MediaAssetDto>>> ListAsync(CancellationToken ct)
    {
        var media = await client.GetMediaAsync(ct);
        return TypedResults.Ok<IReadOnlyList<MediaAssetDto>>(
            media.Where(m => !m.Archived).Select(ToDto).ToList());
    }

    public async Task<Results<Ok<MediaAssetDto>, NotFound>> GetAsync(Guid id, CancellationToken ct)
    {
        var media = await client.GetMediaAsync(id, ct);
        if (media is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(ToDto(media));
    }

    public Task<Results<FileStreamHttpResult, NotFound>> DownloadAsync(Guid id, CancellationToken ct)
    {
        // TODO(escalation): CareerApi returns only a BlobRef (MinIO object key), with no endpoint that
        // streams the binary. The route and response union are preserved, returning 404 until an upstream
        // media-binary surface (or a presigned MinIO URL we can proxy) exists.
        return Task.FromResult<Results<FileStreamHttpResult, NotFound>>(TypedResults.NotFound());
    }

    private static MediaAssetDto ToDto(CareerMediaDto m) => new()
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
