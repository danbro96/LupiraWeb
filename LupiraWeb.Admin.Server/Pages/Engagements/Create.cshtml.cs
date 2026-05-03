using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LupiraWeb.Admin.Server.Pages.Engagements;

public sealed class CreateModel(IDocumentSession session) : PageModel
{
    [BindProperty]
    public EngagementForm Form { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        if (Form.EndDate.HasValue && Form.EndDate < Form.StartDate)
        {
            ModelState.AddModelError(nameof(Form.EndDate), "End date cannot be before start date.");
            return Page();
        }

        var engagementId = Guid.CreateVersion7();
        var titleId = Guid.NewGuid();

        session.Events.StartStream<Engagement>(engagementId,
            new EngagementStarted(
                engagementId,
                Form.Kind,
                Form.Institution.Trim(),
                Form.StartDate,
                Location: null,
                Summary: NullIfBlank(Form.Summary)),
            new TitleAssumed(engagementId, titleId, Form.Title.Trim(), Form.StartDate));

        if (Form.EndDate is DateOnly end)
        {
            session.Events.Append(engagementId,
                new TitleRetired(engagementId, titleId, end),
                new EngagementEnded(engagementId, end, Reason: null));
        }

        await session.SaveChangesAsync(ct);

        return RedirectToPage("Index");
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public sealed class EngagementForm
    {
        [Required, StringLength(200)]
        public string Institution { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public EngagementKind Kind { get; set; } = EngagementKind.Employment;

        [Required]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [DataType(DataType.Date)]
        public DateOnly? EndDate { get; set; }

        [StringLength(2000)]
        public string? Summary { get; set; }
    }
}
