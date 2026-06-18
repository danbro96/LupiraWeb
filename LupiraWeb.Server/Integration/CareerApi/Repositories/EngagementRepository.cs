using LupiraWeb.Domain;
using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Integration.CareerApi.Repositories;

internal sealed class EngagementRepository(ICareerApiClient client) : IEngagementRepository
{
    public async Task<IReadOnlyList<Engagement>> ListAsync(CancellationToken ct)
    {
        var dtos = await client.GetEngagementsAsync(ct);
        return dtos
            .OrderByDescending(e => e.Start)
            .Select(Map)
            .ToList();
    }

    public async Task<Engagement?> GetAsync(Guid id, CancellationToken ct)
    {
        var dto = await client.GetEngagementAsync(id, ct);
        return dto is null ? null : Map(dto);
    }

    private static Engagement Map(CareerEngagementDto e) => new()
    {
        Id = e.Id,
        Kind = e.Kind,
        Institution = e.OrganizationName ?? "",
        Start = e.Start,
        End = e.End,
        Location = e.Location,
        Summary = e.Summary,
        Titles = e.Titles
            .Select(t => new TitleEpoch { TitleId = t.TitleId, Text = t.Text, From = t.From, To = t.To })
            .ToList(),
        SkillIds = e.SkillIds.ToList(),
    };
}
