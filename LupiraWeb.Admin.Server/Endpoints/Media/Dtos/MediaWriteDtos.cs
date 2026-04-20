using LupiraWeb.Domain;

namespace LupiraWeb.Admin.Server.Endpoints.Media.Dtos;

public sealed record MediaUploadResponse
{
    public required Guid MediaId { get; set; }
    public required string BlobRef { get; set; }
}

public sealed record LinkMediaToProjectRequest
{
    public required Guid ProjectId { get; set; }
    public required MediaRole Role { get; set; }
}

public sealed record LinkMediaToSkillsRequest
{
    public required IReadOnlyList<Guid> SkillIds { get; set; }
    public string? Note { get; set; }
}
