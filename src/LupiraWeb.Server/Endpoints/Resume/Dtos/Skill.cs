using LupiraWeb.Server.Contracts;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Endpoints.Resume.Dtos;

public sealed class Skill
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required SkillCategory Category { get; set; }

    public static Skill From(CareerSkillDto s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Category = s.Category,
    };
}
