using LupiraWeb.Domain;

namespace LupiraWeb.Admin.Server.Endpoints.Goals.Dtos;

public sealed record SetGoalRequest
{
    public Guid? SkillId { get; set; }
    public required Maturity TargetMaturity { get; set; }
    public DateOnly? Deadline { get; set; }
    public required string Motivation { get; set; }
}

public sealed record SetGoalResponse
{
    public required Guid GoalId { get; set; }
}

public sealed record RecordProgressRequest
{
    public required string Note { get; set; }
    public Guid? LinkedEventId { get; set; }
}

public sealed record AchieveGoalRequest
{
    public required DateOnly AchievedOn { get; set; }
    public Guid? EvidenceArtifactId { get; set; }
}

public sealed record AbandonGoalRequest
{
    public required DateOnly AbandonedOn { get; set; }
    public required string Reason { get; set; }
}
