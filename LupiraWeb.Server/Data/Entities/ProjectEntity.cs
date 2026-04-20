namespace LupiraWeb.Server.Data.Entities;

public class ProjectEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public Guid? EmploymentId { get; set; }
    public EmploymentEntity? Employment { get; set; }

    public List<ProjectSkillEntity> ProjectSkills { get; set; } = new();
}
