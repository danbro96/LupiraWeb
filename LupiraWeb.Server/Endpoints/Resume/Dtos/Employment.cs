using LupiraWeb.Server.Data.Entities;

namespace LupiraWeb.Server.Endpoints.Resume.Dtos;

public sealed record Employment
{
    public required Guid Id { get; set; }
    public required string Company { get; set; }
    public required string Title { get; set; }
    public required DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Summary { get; set; }
    public string? Location { get; set; }
    public required IReadOnlyList<Skill> Skills { get; set; }
    public required IReadOnlyList<Project> Projects { get; set; }

    public static Employment From(EmploymentEntity e) => new()
    {
        Id = e.Id,
        Company = e.Company,
        Title = e.Title,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        Summary = e.Summary,
        Location = e.Location,
        Skills = e.EmploymentSkills.Select(es => Skill.From(es.Skill)).ToList(),
        Projects = e.Projects.Select(Project.From).ToList(),
    };
}
