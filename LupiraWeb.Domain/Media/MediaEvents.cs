namespace LupiraWeb.Domain;

public record MediaRegistered(
    Guid MediaId,
    string BlobRef,
    string MimeType,
    int? Width,
    int? Height,
    string AltText,
    string? Caption,
    DateTimeOffset OccurredAt);

public record MediaLinkedToProject(
    Guid MediaId,
    Guid ProjectId,
    MediaRole Role,
    DateTimeOffset OccurredAt);

public record MediaLinkedToSkill(
    Guid MediaId,
    Guid SkillId,
    string? Note,
    DateTimeOffset OccurredAt);

public record MediaUnlinked(
    Guid MediaId,
    MediaTargetKind TargetKind,
    Guid TargetId,
    DateTimeOffset OccurredAt);

public record MediaReplaced(
    Guid MediaId,
    string NewBlobRef,
    string NewMimeType,
    int? NewWidth,
    int? NewHeight,
    DateTimeOffset OccurredAt);

public record MediaArchived(
    Guid MediaId,
    string? Reason,
    DateTimeOffset OccurredAt);
