namespace LupiraWeb.Server.Domain;

public enum GoalStatus
{
    Active,
    Achieved,
    Abandoned,
}

public sealed class GoalProgressEntry
{
    public DateTimeOffset RecordedAt { get; set; }
    public string Note { get; set; } = "";
    public Guid? LinkedEventId { get; set; }
}

public class Goal
{
    public Guid Id { get; set; }
    public int Version { get; set; }

    public Guid? SkillId { get; set; }
    public Maturity TargetMaturity { get; set; }
    public DateOnly? Deadline { get; set; }
    public string Motivation { get; set; } = "";
    public GoalStatus Status { get; set; } = GoalStatus.Active;

    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolutionReason { get; set; }
    public Guid? EvidenceArtifactId { get; set; }

    public List<GoalProgressEntry> Progress { get; set; } = new();

    public void Apply(GoalSet e)
    {
        Id = e.GoalId;
        SkillId = e.SkillId;
        TargetMaturity = e.TargetMaturity;
        Deadline = e.Deadline;
        Motivation = e.Motivation;
        Status = GoalStatus.Active;
    }

    public void Apply(GoalRescoped e)
    {
        if (e.NewTargetMaturity is Maturity target) TargetMaturity = target;
        if (e.NewDeadline is DateOnly deadline) Deadline = deadline;
    }

    public void Apply(GoalProgressRecorded e) =>
        Progress.Add(new GoalProgressEntry
        {
            RecordedAt = e.OccurredAt,
            Note = e.Note,
            LinkedEventId = e.LinkedEventId,
        });

    public void Apply(GoalAchieved e)
    {
        Status = GoalStatus.Achieved;
        ResolvedAt = e.OccurredAt;
        EvidenceArtifactId = e.EvidenceArtifactId;
    }

    public void Apply(GoalAbandoned e)
    {
        Status = GoalStatus.Abandoned;
        ResolvedAt = e.OccurredAt;
        ResolutionReason = e.Reason;
    }
}
