namespace LupiraWeb.Server.Contracts;

// Enums and value types shared by the CareerApi wire DTOs and this service's public response DTOs.
// They mirror LupiraCareerApi's canonical definitions; the contract is by enum *name*
// (JsonStringEnumConverter), so the duplication is a stable, string-keyed copy — not a binary coupling.

public enum SkillCategory
{
    Language,
    Framework,
    Tool,
    Platform,
    Method,
    Domain,
    Other,
}

public enum SkillContextKind
{
    InEngagement,
    InProject,
    External,
}

public enum Intensity
{
    Touched,
    Regular,
    Core,
}

public enum Maturity
{
    Aware,
    Working,
    Fluent,
    Expert,
    Teaching,
}

public enum EngagementKind
{
    Employment,
    Study,
    Hobby,
    Volunteer,
    OpenSource,
}

public enum ProjectKind
{
    Professional,
    Personal,
    OpenSource,
    Academic,
}

public enum ProjectStatus
{
    Active,
    Shipped,
    Shelved,
    Archived,
}

public enum ArtifactKind
{
    Repo,
    PullRequest,
    Issue,
    BlogPost,
    Talk,
    Video,
    Certification,
    Paper,
}

public enum ArtifactRole
{
    Evidence,
    Output,
    Source,
}

public enum MediaRole
{
    Hero,
    Gallery,
    Thumbnail,
}

public enum ExperienceKind
{
    Engagement,
    Project,
}

public enum LocationKind
{
    Office,
    Home,
    Client,
    Event,
}

public record Location(LocationKind Kind, string? City, string? Country);
