using LupiraWeb.Server.Contracts;

namespace LupiraWeb.Server.Integration.CareerApi.Dtos;

// Wire DTOs mirroring LupiraCareerApi's read responses. Defined locally (not referenced from the
// CareerApi assembly) so this service owns its view of the contract. The enums and value types
// (EngagementKind, Location, Maturity, …) live in LupiraWeb.Server.Contracts — a string-keyed copy
// of CareerApi's, which keeps the mapping to the public DTOs trivial.

public sealed record CareerProfileDto(
    Guid OwnerPrincipalId,
    string FullName,
    string? Tagline,
    string? Bio,
    string? Location,
    string? GithubUrl,
    string? LinkedInUrl,
    string? WebsiteUrl);

public sealed record CareerTitleEpochDto(Guid TitleId, string Text, DateOnly From, DateOnly? To);

public sealed record CareerEngagementDto(
    Guid Id,
    EngagementKind Kind,
    Guid OrganizationId,
    string? OrganizationName,
    DateOnly Start,
    DateOnly? End,
    Location? Location,
    string? Summary,
    string? CurrentTitle,
    IReadOnlyList<CareerTitleEpochDto> Titles,
    IReadOnlyList<Guid> SkillIds);

public sealed record CareerProjectDto(
    Guid Id,
    ProjectKind Kind,
    string Title,
    string? Description,
    string? Url,
    Guid? EngagementId,
    DateOnly? Start,
    DateOnly? End,
    string? Outcome,
    ProjectStatus Status,
    IReadOnlyList<Guid> SkillIds);

public sealed record CareerSkillDto(
    Guid Id,
    string Name,
    SkillCategory Category,
    IReadOnlyList<string> Aliases,
    Guid? ParentSkillId,
    bool Retired,
    DateOnly? FirstLearnedOn,
    Maturity CurrentMaturity);

public sealed record CareerExperienceItemDto(
    ExperienceKind Kind,
    Guid Id,
    string Title,
    DateOnly OccurredOn,
    DateOnly? EndDate,
    Guid? OrganizationId,
    string? OrganizationName,
    Location? Location,
    IReadOnlyList<Guid> SkillIds);

public sealed record CareerProjectLink(Guid ProjectId, MediaRole Role);

public sealed record CareerMediaDto(
    Guid Id,
    string BlobRef,
    string MimeType,
    int? Width,
    int? Height,
    string AltText,
    string? Caption,
    bool Archived,
    IReadOnlyList<CareerProjectLink> LinkedProjects,
    IReadOnlyList<Guid> LinkedSkillIds);

public sealed record CareerArtifactSkillLink(Guid SkillId, ArtifactRole Role);

public sealed record CareerArtifactDto(
    Guid Id,
    ArtifactKind Kind,
    string Url,
    string Title,
    string? Description,
    DateOnly? ProducedOn,
    bool Archived,
    IReadOnlyList<Guid> LinkedProjectIds,
    IReadOnlyList<Guid> LinkedEngagementIds,
    IReadOnlyList<CareerArtifactSkillLink> LinkedSkills);

public sealed record CareerSkillTimelineEntryDto(
    string Kind,
    DateOnly OccurredOn,
    SkillContextKind? ContextKind,
    Guid? ContextId,
    string? ContextLabel,
    Intensity? Intensity,
    Maturity? Maturity,
    Guid? OtherSkillId,
    string? Note);

public sealed record CareerSkillTimelineDto(
    Guid Id,
    Guid OwnerPrincipalId,
    string Name,
    IReadOnlyList<CareerSkillTimelineEntryDto> Entries);

public sealed record CareerSkillMaturityPointDto(DateOnly OccurredOn, Maturity Maturity, string? Reason);

public sealed record CareerSkillMaturityDto(
    Guid Id,
    Guid OwnerPrincipalId,
    Maturity Current,
    IReadOnlyList<CareerSkillMaturityPointDto> Trajectory);
