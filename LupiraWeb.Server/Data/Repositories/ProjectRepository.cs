using LupiraWeb.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LupiraWeb.Server.Data.Repositories;

internal sealed class ProjectRepository(AppDbContext db) : IProjectRepository
{
    public async Task<IReadOnlyList<ProjectEntity>> ListAsync(CancellationToken ct) =>
        await db.Projects
            .AsNoTracking()
            .Include(p => p.Employment)
            .Include(p => p.ProjectSkills).ThenInclude(ps => ps.Skill)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(ct);

    public Task<ProjectEntity?> GetAsync(Guid id, CancellationToken ct) =>
        db.Projects
            .AsNoTracking()
            .Include(p => p.Employment)
            .Include(p => p.ProjectSkills).ThenInclude(ps => ps.Skill)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
}
