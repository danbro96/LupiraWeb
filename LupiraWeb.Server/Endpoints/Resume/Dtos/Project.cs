using LupiraWeb.Server.Contracts;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Endpoints.Resume.Dtos;

public sealed class Project
{
    public required Guid Id { get; set; }
    public required ProjectKind Kind { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public DateOnly? Start { get; set; }
    public DateOnly? End { get; set; }
    public ProjectStatus Status { get; set; }
    public Guid? EngagementId { get; set; }
    public string? EngagementInstitution { get; set; }
    public required IReadOnlyList<Skill> Skills { get; set; }

    public static Project From(
        CareerProjectDto p,
        IEnumerable<Skill>? skills = null,
        string? engagementInstitution = null) => new()
        {
            Id = p.Id,
            Kind = p.Kind,
            Title = p.Title,
            Description = p.Description,
            Url = p.Url,
            Start = p.Start,
            End = p.End,
            Status = p.Status,
            EngagementId = p.EngagementId,
            EngagementInstitution = engagementInstitution,
            Skills = (skills ?? Array.Empty<Skill>()).ToList(),
        };
}
