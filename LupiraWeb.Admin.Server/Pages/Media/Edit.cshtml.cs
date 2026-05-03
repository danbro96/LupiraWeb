using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LupiraWeb.Admin.Server.Pages.Media;

public sealed class EditModel(IDocumentSession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public MediaAsset? Media { get; private set; }
    public IReadOnlyList<Project> Projects { get; private set; } = Array.Empty<Project>();
    public IReadOnlyList<Skill> Skills { get; private set; } = Array.Empty<Skill>();
    public IReadOnlyDictionary<Guid, Project> ProjectsById { get; private set; } = new Dictionary<Guid, Project>();
    public IReadOnlyDictionary<Guid, Skill> SkillsById { get; private set; } = new Dictionary<Guid, Skill>();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Media = await session.LoadAsync<MediaAsset>(Id, ct);
        if (Media is null) return NotFound();

        await LoadOptionsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostLinkProjectAsync(Guid projectId, MediaRole role, CancellationToken ct)
    {
        Media = await session.LoadAsync<MediaAsset>(Id, ct);
        if (Media is null) return NotFound();

        session.Events.Append(Id, new MediaLinkedToProject(Id, projectId, role, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLinkSkillAsync(Guid skillId, CancellationToken ct)
    {
        Media = await session.LoadAsync<MediaAsset>(Id, ct);
        if (Media is null) return NotFound();

        session.Events.Append(Id, new MediaLinkedToSkill(Id, skillId, Note: null, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    private async Task LoadOptionsAsync(CancellationToken ct)
    {
        Projects = (await session.Query<Project>().ToListAsync(ct))
            .OrderBy(p => p.Title).ToList();
        Skills = (await session.Query<Skill>().ToListAsync(ct))
            .OrderBy(s => s.Name).ToList();

        ProjectsById = Projects.ToDictionary(p => p.Id);
        SkillsById = Skills.ToDictionary(s => s.Id);
    }
}
