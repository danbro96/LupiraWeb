using LupiraWeb.Domain;
using Marten;

namespace LupiraWeb.Server.Data.Repositories;

internal sealed class SkillRepository(IQuerySession session) : ISkillRepository
{
    public async Task<IReadOnlyList<Skill>> ListAsync(CancellationToken ct) =>
        await session.Query<Skill>()
            .Where(s => !s.Retired)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync(ct);
}
