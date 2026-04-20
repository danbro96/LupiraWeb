namespace LupiraWeb.Server.Domain;

// Lifecycle
public record SkillRegistered(
    Guid SkillId,
    string Name,
    SkillCategory Category,
    IReadOnlyList<string>? Aliases,
    Guid? ParentSkillId);

public record SkillRenamed(Guid SkillId, string NewName);

public record SkillCategoryChanged(Guid SkillId, SkillCategory NewCategory);

public record SkillAliasAdded(Guid SkillId, string Alias);

public record SkillReparented(Guid SkillId, Guid? NewParentSkillId);

public record SkillRetired(Guid SkillId);

// Edge events — typed, dated interactions between a skill and the rest of the world.
public record SkillLearned(
    Guid SkillId,
    DateOnly OccurredOn,
    Maturity InitialMaturity,
    SkillEdgeContext Context,
    Evidence? Evidence,
    Location? Location);

public record SkillApplied(
    Guid SkillId,
    DateOnly OccurredOn,
    Intensity Intensity,
    SkillEdgeContext Context,
    Evidence? Evidence,
    Location? Location);

public record SkillDeepened(
    Guid SkillId,
    DateOnly OccurredOn,
    Maturity FromMaturity,
    Maturity ToMaturity,
    string? Note,
    SkillEdgeContext Context,
    Evidence? Evidence,
    Location? Location);

public record SkillTaught(
    Guid SkillId,
    DateOnly OccurredOn,
    string Audience,
    SkillEdgeContext Context,
    Evidence? Evidence,
    Location? Location);

public record SkillReferenced(
    Guid SkillId,
    DateOnly OccurredOn,
    string Note,
    SkillEdgeContext Context,
    Evidence? Evidence,
    Location? Location);

public record SkillsCombined(
    Guid SkillId,
    Guid OtherSkillId,
    DateOnly OccurredOn,
    bool IsPrimary,
    SkillEdgeContext Context,
    Evidence? Evidence);
