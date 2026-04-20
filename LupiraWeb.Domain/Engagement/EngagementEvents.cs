namespace LupiraWeb.Domain;

public record EngagementStarted(
    Guid EngagementId,
    EngagementKind Kind,
    string Institution,
    DateOnly StartDate,
    Location? Location,
    string? Summary);

public record EngagementEnded(Guid EngagementId, DateOnly EndDate, string? Reason);

public record EngagementSummaryRevised(Guid EngagementId, string? Summary);

public record EngagementRelocated(Guid EngagementId, Location NewLocation);

public record EngagementKindReclassified(Guid EngagementId, EngagementKind NewKind);

public record TitleAssumed(Guid EngagementId, Guid TitleId, string Text, DateOnly EffectiveFrom);

public record TitleRevised(Guid EngagementId, Guid TitleId, string NewText);

public record TitleRetired(Guid EngagementId, Guid TitleId, DateOnly EffectiveTo);

// Phase 1 scaffold: simple skill attachments until Phase 2 edge events supersede them.
public record EngagementSkillAttached(Guid EngagementId, Guid SkillId, DateOnly? AttachedOn);

public record EngagementSkillDetached(Guid EngagementId, Guid SkillId);
