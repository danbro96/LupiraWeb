using LupiraWeb.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LupiraWeb.Server.Data.Repositories;

internal sealed class SkillRepository(AppDbContext db) : ISkillRepository
{
    public async Task<IReadOnlyList<SkillEntity>> ListAsync(CancellationToken ct) =>
        await db.Skills
            .AsNoTracking()
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync(ct);
}
