namespace LupiraWeb.Server.Data.Entities;

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

public class SkillEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public SkillCategory Category { get; set; }

    public List<EmploymentSkillEntity> EmploymentSkills { get; set; } = new();
    public List<ProjectSkillEntity> ProjectSkills { get; set; } = new();
}
