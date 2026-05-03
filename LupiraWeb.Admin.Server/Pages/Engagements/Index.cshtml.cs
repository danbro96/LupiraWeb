using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LupiraWeb.Admin.Server.Pages.Engagements;

public sealed class IndexModel(IQuerySession session) : PageModel
{
    public IReadOnlyList<Engagement> Engagements { get; private set; } = Array.Empty<Engagement>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        var all = await session.Query<Engagement>().ToListAsync(ct);
        Engagements = all.OrderByDescending(e => e.Start).ToList();
    }
}
