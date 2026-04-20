using LupiraWeb.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LupiraWeb.Server.Data.Repositories;

internal sealed class EmploymentRepository(AppDbContext db) : IEmploymentRepository
{
    public async Task<IReadOnlyList<EmploymentEntity>> ListAsync(CancellationToken ct) =>
        await db.Employments
            .AsNoTracking()
            .Include(e => e.EmploymentSkills).ThenInclude(es => es.Skill)
            .Include(e => e.Projects)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(ct);

    public Task<EmploymentEntity?> GetAsync(Guid id, CancellationToken ct) =>
        db.Employments
            .AsNoTracking()
            .Include(e => e.EmploymentSkills).ThenInclude(es => es.Skill)
            .Include(e => e.Projects).ThenInclude(p => p.ProjectSkills).ThenInclude(ps => ps.Skill)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
}
