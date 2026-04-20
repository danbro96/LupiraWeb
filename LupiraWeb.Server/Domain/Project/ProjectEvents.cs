namespace LupiraWeb.Server.Domain;

public record ProjectStarted(
    Guid ProjectId,
    ProjectKind Kind,
    string Title,
    string? Description,
    Guid? EngagementId,
    string? Url,
    DateOnly? StartDate);

public record ProjectRenamed(Guid ProjectId, string NewTitle);

public record ProjectDescribed(Guid ProjectId, string? Description);

public record ProjectUrlSet(Guid ProjectId, string? Url);

public record ProjectAttachedToEngagement(Guid ProjectId, Guid EngagementId);

public record ProjectDetachedFromEngagement(Guid ProjectId);

public record ProjectShipped(Guid ProjectId, DateOnly ShippedOn, string? Outcome);

public record ProjectShelved(Guid ProjectId, string? Reason);

public record ProjectArchived(Guid ProjectId);

// Phase 1 scaffold: simple skill attachments until Phase 2 edge events supersede them.
public record ProjectSkillAttached(Guid ProjectId, Guid SkillId, DateOnly? AttachedOn);

public record ProjectSkillDetached(Guid ProjectId, Guid SkillId);
