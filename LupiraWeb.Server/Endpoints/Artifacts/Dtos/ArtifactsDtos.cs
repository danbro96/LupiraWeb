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
