namespace LupiraWeb.Server.Domain;

public record ArtifactRegistered(
    Guid ArtifactId,
    ArtifactKind Kind,
    string Url,
    string Title,
    string? Description,
    DateOnly? ProducedOn,
    DateTimeOffset OccurredAt);

public record ArtifactUpdated(
    Guid ArtifactId,
    string? NewUrl,
    string? NewTitle,
    string? NewDescription,
    DateTimeOffset OccurredAt);

public record ArtifactLinkedToProject(
    Guid ArtifactId,
    Guid ProjectId,
    DateTimeOffset OccurredAt);

public record ArtifactLinkedToSkill(
    Guid ArtifactId,
    Guid SkillId,
    ArtifactRole Role,
    DateTimeOffset OccurredAt);

public record ArtifactLinkedToEngagement(
    Guid ArtifactId,
    Guid EngagementId,
    DateTimeOffset OccurredAt);

public record ArtifactUnlinked(
    Guid ArtifactId,
    ArtifactTargetKind TargetKind,
    Guid TargetId,
    DateTimeOffset OccurredAt);

public record ArtifactArchived(
    Guid ArtifactId,
    string? Reason,
    DateTimeOffset OccurredAt);
