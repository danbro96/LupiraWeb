using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LupiraWeb.Admin.Server.Pages.Projects;

public sealed class EditModel(IDocumentSession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public ProjectForm Form { get; set; } = new();

    public Project? Project { get; private set; }
    public IReadOnlyList<Engagement> Engagements { get; private set; } = Array.Empty<Engagement>();
    public bool Saved { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Project = await session.LoadAsync<Project>(Id, ct);
        if (Project is null) return NotFound();

        await LoadEngagementsAsync(ct);
        Form = ProjectForm.From(Project);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Project = await session.LoadAsync<Project>(Id, ct);
        if (Project is null) return NotFound();

        await LoadEngagementsAsync(ct);

        if (!ModelState.IsValid) return Page();

        var changed = false;

        var newTitle = Form.Title.Trim();
        if (!string.Equals(Project.Title, newTitle, StringComparison.Ordinal))
        {
            session.Events.Append(Id, new ProjectRenamed(Id, newTitle));
            changed = true;
        }

        var newDescription = NullIfBlank(Form.Description);
        if (!string.Equals(Project.Description, newDescription, StringComparison.Ordinal))
        {
            session.Events.Append(Id, new ProjectDescribed(Id, newDescription));
            changed = true;
        }

        var newUrl = NullIfBlank(Form.Url);
        if (!string.Equals(Project.Url, newUrl, StringComparison.Ordinal))
        {
            session.Events.Append(Id, new ProjectUrlSet(Id, newUrl));
            changed = true;
        }

        if (Project.EngagementId != Form.EngagementId)
        {
            if (Form.EngagementId is Guid newEid)
                session.Events.Append(Id, new ProjectAttachedToEngagement(Id, newEid));
            else
                session.Events.Append(Id, new ProjectDetachedFromEngagement(Id));
            changed = true;
        }

        if (changed)
            await session.SaveChangesAsync(ct);

        Project = await session.LoadAsync<Project>(Id, ct);
        Form = ProjectForm.From(Project!);
        Saved = true;
        return Page();
    }

    public async Task<IActionResult> OnPostShipAsync(DateOnly shippedOn, string? outcome, CancellationToken ct)
    {
        var project = await session.LoadAsync<Project>(Id, ct);
        if (project is null) return NotFound();

        session.Events.Append(Id, new ProjectShipped(Id, shippedOn, NullIfBlank(outcome)));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostShelveAsync(string? reason, CancellationToken ct)
    {
        var project = await session.LoadAsync<Project>(Id, ct);
        if (project is null) return NotFound();

        session.Events.Append(Id, new ProjectShelved(Id, NullIfBlank(reason)));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostArchiveAsync(CancellationToken ct)
    {
        var project = await session.LoadAsync<Project>(Id, ct);
        if (project is null) return NotFound();

        session.Events.Append(Id, new ProjectArchived(Id));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    private async Task LoadEngagementsAsync(CancellationToken ct)
    {
        var all = await session.Query<Engagement>().ToListAsync(ct);
        Engagements = all.OrderBy(e => e.Institution).ToList();
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public sealed class ProjectForm
    {
        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Url, StringLength(500)]
        public string? Url { get; set; }

        public Guid? EngagementId { get; set; }

        public static ProjectForm From(Project p) => new()
        {
            Title = p.Title,
            Description = p.Description,
            Url = p.Url,
            EngagementId = p.EngagementId,
        };
    }
}
