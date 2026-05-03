using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LupiraWeb.Admin.Server.Pages.Goals;

public sealed class IndexModel(IQuerySession session) : PageModel
{
    public IReadOnlyList<Goal> Goals { get; private set; } = Array.Empty<Goal>();
    public IReadOnlyDictionary<Guid, Skill> SkillsById { get; private set; } = new Dictionary<Guid, Skill>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        var goals = await session.Query<Goal>().ToListAsync(ct);
        var skills = await session.Query<Skill>().ToListAsync(ct);

        Goals = goals
            .OrderBy(g => g.Status == GoalStatus.Active ? 0 : 1)
            .ThenByDescending(g => g.Deadline ?? DateOnly.MaxValue)
            .ToList();
        SkillsById = skills.ToDictionary(s => s.Id);
    }
}
