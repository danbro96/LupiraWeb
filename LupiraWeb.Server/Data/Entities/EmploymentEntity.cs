namespace LupiraWeb.Server.Data.Entities;

public class EmploymentEntity
{
    public Guid Id { get; set; }
    public required string Company { get; set; }
    public required string Title { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Summary { get; set; }
    public string? Location { get; set; }

    public List<ProjectEntity> Projects { get; set; } = new();
    public List<EmploymentSkillEntity> EmploymentSkills { get; set; } = new();
}
