using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LupiraWeb.Admin.Server.Pages.Engagements;

public sealed class EditModel(IDocumentSession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public EngagementForm Form { get; set; } = new();

    public Engagement? Engagement { get; private set; }
    public bool Saved { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Engagement = await session.LoadAsync<Engagement>(Id, ct);
        if (Engagement is null) return NotFound();

        Form = EngagementForm.From(Engagement);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Engagement = await session.LoadAsync<Engagement>(Id, ct);
        if (Engagement is null) return NotFound();

        if (!ModelState.IsValid) return Page();

        if (Form.EndDate.HasValue && Form.EndDate < Form.StartDate)
        {
            ModelState.AddModelError(nameof(Form.EndDate), "End date cannot be before start date.");
            return Page();
        }

        var changed = false;

        if (Engagement.Kind != Form.Kind)
        {
            session.Events.Append(Id, new EngagementKindReclassified(Id, Form.Kind));
            changed = true;
        }

        var newSummary = NullIfBlank(Form.Summary);
        if (!string.Equals(Engagement.Summary, newSummary, StringComparison.Ordinal))
        {
            session.Events.Append(Id, new EngagementSummaryRevised(Id, newSummary));
            changed = true;
        }

        // Institution and StartDate aren't expressible as discrete events in the current
        // event model — skipped intentionally. If you need to change those, do it via SQL
        // or extend the event model.

        if (Engagement.End != Form.EndDate)
        {
            if (Form.EndDate is DateOnly end)
            {
                session.Events.Append(Id, new EngagementEnded(Id, end, Reason: null));
                changed = true;
            }
            // Reopening (clearing End) requires a new event; out of scope.
        }

        if (changed)
            await session.SaveChangesAsync(ct);

        Engagement = await session.LoadAsync<Engagement>(Id, ct);
        Form = EngagementForm.From(Engagement!);
        Saved = true;
        return Page();
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public sealed class EngagementForm
    {
        [Required, StringLength(200)]
        public string Institution { get; set; } = string.Empty;

        [Required]
        public EngagementKind Kind { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? EndDate { get; set; }

        [StringLength(2000)]
        public string? Summary { get; set; }

        public static EngagementForm From(Engagement e) => new()
        {
            Institution = e.Institution,
            Kind = e.Kind,
            StartDate = e.Start,
            EndDate = e.End,
            Summary = e.Summary,
        };
    }
}
