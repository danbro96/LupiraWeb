using LupiraWeb.Server.Data.Entities;

namespace LupiraWeb.Server.Endpoints.Resume.Dtos;

public sealed record Project
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? EmploymentId { get; set; }
    public string? EmploymentCompany { get; set; }
    public required IReadOnlyList<Skill> Skills { get; set; }

    public static Project From(ProjectEntity p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Description = p.Description,
        Url = p.Url,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        EmploymentId = p.EmploymentId,
        EmploymentCompany = p.Employment?.Company,
        Skills = p.ProjectSkills.Select(ps => Skill.From(ps.Skill)).ToList(),
    };
}
