using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LupiraWeb.Admin.Server.Pages.Media;

public sealed class IndexModel(IQuerySession session) : PageModel
{
    public IReadOnlyList<MediaAsset> Media { get; private set; } = Array.Empty<MediaAsset>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        var all = await session.Query<MediaAsset>().ToListAsync(ct);
        Media = all.Where(m => !m.Archived).OrderBy(m => m.AltText).ToList();
    }
}
