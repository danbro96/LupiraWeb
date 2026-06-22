using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Integration.CareerApi.Repositories;

internal sealed class SkillRepository(ICareerApiClient client) : ISkillRepository
{
    public async Task<IReadOnlyList<CareerSkillDto>> ListAsync(CancellationToken ct)
    {
        var dtos = await client.GetSkillsAsync(ct);
        return dtos
            .Where(s => !s.Retired)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToList();
    }
}
