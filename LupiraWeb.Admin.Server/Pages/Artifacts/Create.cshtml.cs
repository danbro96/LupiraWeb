using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LupiraWeb.Admin.Server.Pages.Artifacts;

public sealed class CreateModel(IDocumentSession session) : PageModel
{
    [BindProperty]
    public ArtifactForm Form { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var artifactId = Guid.CreateVersion7();
        session.Events.StartStream<Artifact>(artifactId,
            new ArtifactRegistered(
                artifactId,
                Form.Kind,
                Form.Url.Trim(),
                Form.Title.Trim(),
                NullIfBlank(Form.Description),
                Form.ProducedOn,
                DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);

        return RedirectToPage("Edit", new { id = artifactId });
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public sealed class ArtifactForm
    {
        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, Url, StringLength(500)]
        public string Url { get; set; } = string.Empty;

        [Required]
        public ArtifactKind Kind { get; set; } = ArtifactKind.Repo;

        [StringLength(1000)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? ProducedOn { get; set; }
    }
}
