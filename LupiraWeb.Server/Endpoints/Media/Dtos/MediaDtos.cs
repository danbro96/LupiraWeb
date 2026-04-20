using LupiraWeb.Domain;

namespace LupiraWeb.Server.Endpoints.Media.Dtos;

public sealed record MediaAssetDto
{
    public required Guid Id { get; set; }
    public required string BlobRef { get; set; }
    public required string MimeType { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public required string AltText { get; set; }
    public string? Caption { get; set; }
    public bool Archived { get; set; }
    public required IReadOnlyList<MediaProjectLinkDto> LinkedProjects { get; set; }
    public required IReadOnlyList<Guid> LinkedSkillIds { get; set; }
}

public sealed record MediaProjectLinkDto
{
    public required Guid ProjectId { get; set; }
    public required MediaRole Role { get; set; }
}
