using LupiraWeb.Domain;

namespace LupiraWeb.Server.Endpoints.Skills.Dtos;

public sealed record SkillTimelineResponse
{
    public required Guid SkillId { get; set; }
    public required string Name { get; set; }
    public required IReadOnlyList<SkillTimelineEntryDto> Entries { get; set; }
}

public sealed record SkillTimelineEntryDto
{
    public required string Kind { get; set; }
    public required DateOnly OccurredOn { get; set; }
    public SkillContextKind? ContextKind { get; set; }
    public Guid? ContextId { get; set; }
    public string? ContextLabel { get; set; }
    public Intensity? Intensity { get; set; }
    public Maturity? Maturity { get; set; }
    public Guid? OtherSkillId { get; set; }
    public string? Note { get; set; }
}

public sealed record SkillRelatedResponse
{
    public required Guid SkillId { get; set; }
    public required IReadOnlyList<SkillRelatedEntry> Related { get; set; }
}

public sealed record SkillRelatedEntry
{
    public required Guid SkillId { get; set; }
    public required string Name { get; set; }
    public required int Count { get; set; }
    public DateOnly? FirstSeen { get; set; }
    public DateOnly? LastSeen { get; set; }
}

public sealed record SkillMaturityResponse
{
    public required Guid SkillId { get; set; }
    public required Maturity Current { get; set; }
    public required IReadOnlyList<SkillMaturityPointDto> Trajectory { get; set; }
}

public sealed record SkillMaturityPointDto
{
    public required DateOnly OccurredOn { get; set; }
    public required Maturity Maturity { get; set; }
    public string? Reason { get; set; }
}
