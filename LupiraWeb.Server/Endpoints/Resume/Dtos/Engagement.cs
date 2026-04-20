using LupiraWeb.Domain;
using EngagementDocument = LupiraWeb.Domain.Engagement;

namespace LupiraWeb.Server.Endpoints.Resume.Dtos;

public sealed record Engagement
{
    public required Guid Id { get; set; }
    public required EngagementKind Kind { get; set; }
    public required string Institution { get; set; }
    public string? Title { get; set; }
    public required IReadOnlyList<TitleEpochDto> Titles { get; set; }
    public required DateOnly Start { get; set; }
    public DateOnly? End { get; set; }
    public string? Summary { get; set; }
    public Location? Location { get; set; }
    public required IReadOnlyList<Skill> Skills { get; set; }
    public required IReadOnlyList<Project> Projects { get; set; }

    public static Engagement From(
        EngagementDocument e,
        IEnumerable<Skill> skills,
        IEnumerable<Project> projects) => new()
    {
        Id = e.Id,
        Kind = e.Kind,
        Institution = e.Institution,
        Title = e.CurrentTitle,
        Titles = e.Titles.Select(t => new TitleEpochDto
        {
            TitleId = t.TitleId,
            Text = t.Text,
            From = t.From,
            To = t.To,
        }).ToList(),
        Start = e.Start,
        End = e.End,
        Summary = e.Summary,
        Location = e.Location,
        Skills = skills.ToList(),
        Projects = projects.ToList(),
    };
}

public sealed record TitleEpochDto
{
    public required Guid TitleId { get; set; }
    public required string Text { get; set; }
    public required DateOnly From { get; set; }
    public DateOnly? To { get; set; }
}
