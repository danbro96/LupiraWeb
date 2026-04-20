namespace LupiraWeb.Server.Domain;

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

public class Project
{
    public Guid Id { get; set; }
    public int Version { get; set; }

    public ProjectKind Kind { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Url { get; set; }
    public Guid? EngagementId { get; set; }
    public DateOnly? Start { get; set; }
    public DateOnly? End { get; set; }
    public string? Outcome { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public List<Guid> SkillIds { get; set; } = new();

    public void Apply(ProjectStarted e)
    {
        Id = e.ProjectId;
        Kind = e.Kind;
        Title = e.Title;
        Description = e.Description;
        EngagementId = e.EngagementId;
        Url = e.Url;
        Start = e.StartDate;
    }

    public void Apply(ProjectRenamed e) => Title = e.NewTitle;
    public void Apply(ProjectDescribed e) => Description = e.Description;
    public void Apply(ProjectUrlSet e) => Url = e.Url;
    public void Apply(ProjectAttachedToEngagement e) => EngagementId = e.EngagementId;
    public void Apply(ProjectDetachedFromEngagement e) => EngagementId = null;

    public void Apply(ProjectShipped e)
    {
        End = e.ShippedOn;
        Outcome = e.Outcome;
        Status = ProjectStatus.Shipped;
    }

    public void Apply(ProjectShelved e)
    {
        Outcome = e.Reason;
        Status = ProjectStatus.Shelved;
    }

    public void Apply(ProjectArchived e) => Status = ProjectStatus.Archived;

    public void Apply(ProjectSkillAttached e)
    {
        if (!SkillIds.Contains(e.SkillId))
            SkillIds.Add(e.SkillId);
    }

    public void Apply(ProjectSkillDetached e) => SkillIds.Remove(e.SkillId);
}
