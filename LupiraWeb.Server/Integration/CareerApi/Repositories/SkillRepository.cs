using LupiraWeb.Domain;
using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Integration.CareerApi.Repositories;

internal sealed class SkillRepository(ICareerApiClient client) : ISkillRepository
{
    public async Task<IReadOnlyList<Skill>> ListAsync(CancellationToken ct)
    {
        var dtos = await client.GetSkillsAsync(ct);
        return dtos
            .Where(s => !s.Retired)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .Select(Map)
            .ToList();
    }

    private static Skill Map(CareerSkillDto s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Category = s.Category,
        Aliases = s.Aliases.ToList(),
        ParentSkillId = s.ParentSkillId,
        Retired = s.Retired,
        FirstLearnedOn = s.FirstLearnedOn,
        CurrentMaturity = s.CurrentMaturity,
    };
}
