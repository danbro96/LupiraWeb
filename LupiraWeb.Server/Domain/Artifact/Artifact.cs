namespace LupiraWeb.Server.Domain;

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

public enum ArtifactTargetKind
{
    Project,
    Skill,
    Engagement,
}

public class Artifact
{
    public Guid Id { get; set; }
    public int Version { get; set; }

    public ArtifactKind Kind { get; set; }
    public string Url { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public DateOnly? ProducedOn { get; set; }
    public bool Archived { get; set; }

    public List<Guid> LinkedProjectIds { get; set; } = new();
    public List<Guid> LinkedEngagementIds { get; set; } = new();
    public List<ArtifactSkillLink> LinkedSkills { get; set; } = new();

    public void Apply(ArtifactRegistered e)
    {
        Id = e.ArtifactId;
        Kind = e.Kind;
        Url = e.Url;
        Title = e.Title;
        Description = e.Description;
        ProducedOn = e.ProducedOn;
    }

    public void Apply(ArtifactUpdated e)
    {
        if (e.NewUrl is not null) Url = e.NewUrl;
        if (e.NewTitle is not null) Title = e.NewTitle;
        if (e.NewDescription is not null) Description = e.NewDescription;
    }

    public void Apply(ArtifactLinkedToProject e)
    {
        if (!LinkedProjectIds.Contains(e.ProjectId))
            LinkedProjectIds.Add(e.ProjectId);
    }

    public void Apply(ArtifactLinkedToEngagement e)
    {
        if (!LinkedEngagementIds.Contains(e.EngagementId))
            LinkedEngagementIds.Add(e.EngagementId);
    }

    public void Apply(ArtifactLinkedToSkill e)
    {
        if (LinkedSkills.All(l => l.SkillId != e.SkillId))
            LinkedSkills.Add(new ArtifactSkillLink { SkillId = e.SkillId, Role = e.Role });
    }

    public void Apply(ArtifactUnlinked e)
    {
        switch (e.TargetKind)
        {
            case ArtifactTargetKind.Project:
                LinkedProjectIds.Remove(e.TargetId);
                break;
            case ArtifactTargetKind.Engagement:
                LinkedEngagementIds.Remove(e.TargetId);
                break;
            case ArtifactTargetKind.Skill:
                LinkedSkills.RemoveAll(l => l.SkillId == e.TargetId);
                break;
        }
    }

    public void Apply(ArtifactArchived e) => Archived = true;
}

public sealed class ArtifactSkillLink
{
    public Guid SkillId { get; set; }
    public ArtifactRole Role { get; set; }
}
