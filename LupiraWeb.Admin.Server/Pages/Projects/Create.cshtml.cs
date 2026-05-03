using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LupiraWeb.Admin.Server.Pages.Projects;

public sealed class CreateModel(IDocumentSession session) : PageModel
{
    [BindProperty]
    public ProjectForm Form { get; set; } = new();

    public IReadOnlyList<Engagement> Engagements { get; private set; } = Array.Empty<Engagement>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadEngagementsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadEngagementsAsync(ct);

        if (!ModelState.IsValid)
            return Page();

        var projectId = Guid.CreateVersion7();
        session.Events.StartStream<Project>(projectId,
            new ProjectStarted(
                projectId,
                Form.Kind,
                Form.Title.Trim(),
                NullIfBlank(Form.Description),
                Form.EngagementId,
                NullIfBlank(Form.Url),
                Form.StartDate));
        await session.SaveChangesAsync(ct);

        return RedirectToPage("Index");
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

        [Required]
        public ProjectKind Kind { get; set; } = ProjectKind.Personal;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Url, StringLength(500)]
        public string? Url { get; set; }

        public Guid? EngagementId { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? StartDate { get; set; }
    }
}
