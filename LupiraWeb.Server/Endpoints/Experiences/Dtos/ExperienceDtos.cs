using LupiraWeb.Domain;

namespace LupiraWeb.Server.Endpoints.Experiences.Dtos;

public sealed record ExperienceDto
{
    public required Guid Id { get; set; }
    public required ExperienceKind Kind { get; set; }
    public required string Title { get; set; }
    public required DateOnly OccurredOn { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? EngagementId { get; set; }
    public Guid? ProjectId { get; set; }
    public required IReadOnlyList<Guid> SkillIds { get; set; }
    public Location? Location { get; set; }
}
