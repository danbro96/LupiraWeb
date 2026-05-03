using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LupiraWeb.Admin.Server.Pages.Skills;

public sealed class IndexModel(IQuerySession session) : PageModel
{
    public IReadOnlyList<Skill> Skills { get; private set; } = Array.Empty<Skill>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Skills = await session.Query<Skill>()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }
}
