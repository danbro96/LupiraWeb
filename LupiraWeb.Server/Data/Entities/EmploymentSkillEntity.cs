namespace LupiraWeb.Server.Data.Entities;

public class EmploymentSkillEntity
{
    public Guid EmploymentId { get; set; }
    public EmploymentEntity Employment { get; set; } = null!;

    public Guid SkillId { get; set; }
    public SkillEntity Skill { get; set; } = null!;

    public DateTimeOffset GainedAt { get; set; }
}
