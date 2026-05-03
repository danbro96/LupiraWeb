using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LupiraWeb.Admin.Server.Pages.Artifacts;

public sealed class EditModel(IDocumentSession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public Artifact? Artifact { get; private set; }
    public IReadOnlyList<Project> Projects { get; private set; } = Array.Empty<Project>();
    public IReadOnlyList<Engagement> Engagements { get; private set; } = Array.Empty<Engagement>();
    public IReadOnlyList<Skill> Skills { get; private set; } = Array.Empty<Skill>();
    public IReadOnlyDictionary<Guid, Project> ProjectsById { get; private set; } = new Dictionary<Guid, Project>();
    public IReadOnlyDictionary<Guid, Engagement> EngagementsById { get; private set; } = new Dictionary<Guid, Engagement>();
    public IReadOnlyDictionary<Guid, Skill> SkillsById { get; private set; } = new Dictionary<Guid, Skill>();
    public bool Saved { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Artifact = await session.LoadAsync<Artifact>(Id, ct);
        if (Artifact is null) return NotFound();

        await LoadOptionsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostLinkProjectAsync(Guid projectId, CancellationToken ct)
    {
        Artifact = await session.LoadAsync<Artifact>(Id, ct);
        if (Artifact is null) return NotFound();

        session.Events.Append(Id, new ArtifactLinkedToProject(Id, projectId, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLinkEngagementAsync(Guid engagementId, CancellationToken ct)
    {
        Artifact = await session.LoadAsync<Artifact>(Id, ct);
        if (Artifact is null) return NotFound();

        session.Events.Append(Id, new ArtifactLinkedToEngagement(Id, engagementId, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLinkSkillAsync(Guid skillId, ArtifactRole role, CancellationToken ct)
    {
        Artifact = await session.LoadAsync<Artifact>(Id, ct);
        if (Artifact is null) return NotFound();

        session.Events.Append(Id, new ArtifactLinkedToSkill(Id, skillId, role, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    private async Task LoadOptionsAsync(CancellationToken ct)
    {
        Projects = (await session.Query<Project>().ToListAsync(ct))
            .OrderBy(p => p.Title).ToList();
        Engagements = (await session.Query<Engagement>().ToListAsync(ct))
            .OrderBy(e => e.Institution).ToList();
        Skills = (await session.Query<Skill>().ToListAsync(ct))
            .OrderBy(s => s.Name).ToList();

        ProjectsById = Projects.ToDictionary(p => p.Id);
        EngagementsById = Engagements.ToDictionary(e => e.Id);
        SkillsById = Skills.ToDictionary(s => s.Id);
    }
}
