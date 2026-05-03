using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LupiraWeb.Admin.Server.Pages.Artifacts;

public sealed class IndexModel(IQuerySession session) : PageModel
{
    public IReadOnlyList<Artifact> Artifacts { get; private set; } = Array.Empty<Artifact>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        var all = await session.Query<Artifact>().ToListAsync(ct);
        Artifacts = all.OrderByDescending(a => a.ProducedOn ?? DateOnly.MinValue).ToList();
    }
}
