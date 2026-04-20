using LupiraWeb.Domain;

namespace LupiraWeb.Server.Endpoints.Artifacts.Dtos;

public sealed record ArtifactDto
{
    public required Guid Id { get; set; }
    public required ArtifactKind Kind { get; set; }
    public required string Url { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ProducedOn { get; set; }
    public bool Archived { get; set; }
    public required IReadOnlyList<Guid> LinkedProjectIds { get; set; }
    public required IReadOnlyList<Guid> LinkedEngagementIds { get; set; }
    public required IReadOnlyList<ArtifactSkillLinkDto> LinkedSkills { get; set; }
}

public sealed record ArtifactSkillLinkDto
{
    public required Guid SkillId { get; set; }
    public required ArtifactRole Role { get; set; }
}

public sealed record RegisterArtifactRequest
{
    public required ArtifactKind Kind { get; set; }
    public required string Url { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ProducedOn { get; set; }
}

public sealed record RegisterArtifactResponse
{
    public required Guid ArtifactId { get; set; }
}

public sealed record LinkArtifactToProjectRequest
{
    public required Guid ProjectId { get; set; }
}

public sealed record LinkArtifactToEngagementRequest
{
    public required Guid EngagementId { get; set; }
}

public sealed record LinkArtifactToSkillRequest
{
    public required Guid SkillId { get; set; }
    public required ArtifactRole Role { get; set; }
}
