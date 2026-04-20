namespace LupiraWeb.Server.Data.Entities;

public class ProjectSkillEntity
{
    public Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; } = null!;

    public Guid SkillId { get; set; }
    public SkillEntity Skill { get; set; } = null!;

    public DateTimeOffset GainedAt { get; set; }
}
