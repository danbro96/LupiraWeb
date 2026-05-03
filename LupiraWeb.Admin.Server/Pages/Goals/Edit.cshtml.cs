using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LupiraWeb.Admin.Server.Pages.Goals;

public sealed class EditModel(IDocumentSession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public Goal? Goal { get; private set; }
    public IReadOnlyDictionary<Guid, Skill> SkillsById { get; private set; } = new Dictionary<Guid, Skill>();
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await LoadAsync(ct);
        return Goal is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostProgressAsync(string note, CancellationToken ct)
    {
        await LoadAsync(ct);
        if (Goal is null) return NotFound();

        if (Goal.Status != GoalStatus.Active)
        {
            ErrorMessage = "Goal is already resolved.";
            return Page();
        }

        session.Events.Append(Id, new GoalProgressRecorded(Id, note, LinkedEventId: null, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAchieveAsync(DateOnly achievedOn, CancellationToken ct)
    {
        await LoadAsync(ct);
        if (Goal is null) return NotFound();

        if (Goal.Status != GoalStatus.Active)
        {
            ErrorMessage = "Goal is already resolved.";
            return Page();
        }

        session.Events.Append(Id, new GoalAchieved(Id, achievedOn, EvidenceArtifactId: null, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAbandonAsync(DateOnly abandonedOn, string reason, CancellationToken ct)
    {
        await LoadAsync(ct);
        if (Goal is null) return NotFound();

        if (Goal.Status != GoalStatus.Active)
        {
            ErrorMessage = "Goal is already resolved.";
            return Page();
        }

        session.Events.Append(Id, new GoalAbandoned(Id, abandonedOn, reason, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        Goal = await session.LoadAsync<Goal>(Id, ct);
        var skills = await session.Query<Skill>().ToListAsync(ct);
        SkillsById = skills.ToDictionary(s => s.Id);
    }
}
