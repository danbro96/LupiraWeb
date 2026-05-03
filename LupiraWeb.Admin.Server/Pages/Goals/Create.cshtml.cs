using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LupiraWeb.Admin.Server.Pages.Goals;

public sealed class CreateModel(IDocumentSession session) : PageModel
{
    [BindProperty]
    public GoalForm Form { get; set; } = new();

    public IReadOnlyList<Skill> Skills { get; private set; } = Array.Empty<Skill>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadSkillsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadSkillsAsync(ct);

        if (!ModelState.IsValid)
            return Page();

        var goalId = Guid.CreateVersion7();
        session.Events.StartStream<Goal>(goalId,
            new GoalSet(
                goalId,
                Form.SkillId,
                Form.TargetMaturity,
                Form.Deadline,
                Form.Motivation.Trim(),
                DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);

        return RedirectToPage("Edit", new { id = goalId });
    }

    private async Task LoadSkillsAsync(CancellationToken ct)
    {
        var all = await session.Query<Skill>().ToListAsync(ct);
        Skills = all.OrderBy(s => s.Name).ToList();
    }

    public sealed class GoalForm
    {
        public Guid? SkillId { get; set; }

        [Required]
        public Maturity TargetMaturity { get; set; } = Maturity.Working;

        [DataType(DataType.Date)]
        public DateOnly? Deadline { get; set; }

        [Required, StringLength(1000)]
        public string Motivation { get; set; } = string.Empty;
    }
}
