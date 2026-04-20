using Marten.Events.Projections;

namespace LupiraWeb.Server.Domain;

public enum ExperienceKind
{
    Engagement,
    Project,
}

public sealed class ExperienceRow
{
    public Guid Id { get; set; }
    public ExperienceKind Kind { get; set; }
    public string Title { get; set; } = "";
    public DateOnly OccurredOn { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? EngagementId { get; set; }
    public Guid? ProjectId { get; set; }
    public List<Guid> SkillIds { get; set; } = new();
    public Location? Location { get; set; }
}

public sealed class ExperienceProjection : MultiStreamProjection<ExperienceRow, Guid>
{
    public ExperienceProjection()
    {
        Identity<EngagementStarted>(e => e.EngagementId);
        Identity<EngagementEnded>(e => e.EngagementId);
        Identity<EngagementRelocated>(e => e.EngagementId);
        Identity<EngagementSkillAttached>(e => e.EngagementId);
        Identity<EngagementSkillDetached>(e => e.EngagementId);

        Identity<ProjectStarted>(e => e.ProjectId);
        Identity<ProjectShipped>(e => e.ProjectId);
        Identity<ProjectShelved>(e => e.ProjectId);
        Identity<ProjectSkillAttached>(e => e.ProjectId);
        Identity<ProjectSkillDetached>(e => e.ProjectId);
    }

    public ExperienceRow Create(EngagementStarted e) => new()
    {
        Id = e.EngagementId,
        Kind = ExperienceKind.Engagement,
        Title = e.Institution,
        OccurredOn = e.StartDate,
        EngagementId = e.EngagementId,
        Location = e.Location,
    };

    public ExperienceRow Create(ProjectStarted e) => new()
    {
        Id = e.ProjectId,
        Kind = ExperienceKind.Project,
        Title = e.Title,
        OccurredOn = e.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
        EngagementId = e.EngagementId,
        ProjectId = e.ProjectId,
    };

    public void Apply(EngagementEnded e, ExperienceRow row) => row.EndDate = e.EndDate;
    public void Apply(EngagementRelocated e, ExperienceRow row) => row.Location = e.NewLocation;

    public void Apply(EngagementSkillAttached e, ExperienceRow row)
    {
        if (!row.SkillIds.Contains(e.SkillId))
            row.SkillIds.Add(e.SkillId);
    }

    public void Apply(EngagementSkillDetached e, ExperienceRow row) =>
        row.SkillIds.Remove(e.SkillId);

    public void Apply(ProjectShipped e, ExperienceRow row) => row.EndDate = e.ShippedOn;

    public void Apply(ProjectSkillAttached e, ExperienceRow row)
    {
        if (!row.SkillIds.Contains(e.SkillId))
            row.SkillIds.Add(e.SkillId);
    }

    public void Apply(ProjectSkillDetached e, ExperienceRow row) =>
        row.SkillIds.Remove(e.SkillId);
}
