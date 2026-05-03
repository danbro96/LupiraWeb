using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LupiraWeb.Admin.Server.Pages.Projects;

public sealed class IndexModel(IQuerySession session) : PageModel
{
    public IReadOnlyList<Project> Projects { get; private set; } = Array.Empty<Project>();
    public IReadOnlyDictionary<Guid, Engagement> EngagementsById { get; private set; }
        = new Dictionary<Guid, Engagement>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        var projects = await session.Query<Project>().ToListAsync(ct);
        var engagements = await session.Query<Engagement>().ToListAsync(ct);

        Projects = projects.OrderByDescending(p => p.Start ?? DateOnly.MinValue).ToList();
        EngagementsById = engagements.ToDictionary(e => e.Id);
    }
}
