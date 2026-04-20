namespace LupiraWeb.Server.Domain;

public record GoalSet(
    Guid GoalId,
    Guid? SkillId,
    Maturity TargetMaturity,
    DateOnly? Deadline,
    string Motivation,
    DateTimeOffset OccurredAt);

public record GoalRescoped(
    Guid GoalId,
    Maturity? NewTargetMaturity,
    DateOnly? NewDeadline,
    DateTimeOffset OccurredAt);

public record GoalProgressRecorded(
    Guid GoalId,
    string Note,
    Guid? LinkedEventId,
    DateTimeOffset OccurredAt);

public record GoalAchieved(
    Guid GoalId,
    DateOnly AchievedOn,
    Guid? EvidenceArtifactId,
    DateTimeOffset OccurredAt);

public record GoalAbandoned(
    Guid GoalId,
    DateOnly AbandonedOn,
    string Reason,
    DateTimeOffset OccurredAt);
