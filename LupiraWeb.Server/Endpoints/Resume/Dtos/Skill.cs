using LupiraWeb.Server.Domain;
using SkillDocument = LupiraWeb.Server.Domain.Skill;

namespace LupiraWeb.Server.Endpoints.Resume.Dtos;

public sealed record Skill
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required SkillCategory Category { get; set; }

    public static Skill From(SkillDocument s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Category = s.Category,
    };
}
