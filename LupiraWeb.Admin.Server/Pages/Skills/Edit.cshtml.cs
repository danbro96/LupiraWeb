using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LupiraWeb.Admin.Server.Pages.Skills;

public sealed class EditModel(IDocumentSession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public SkillForm Form { get; set; } = new();

    public Skill? Skill { get; private set; }
    public bool Saved { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Skill = await session.LoadAsync<Skill>(Id, ct);
        if (Skill is null) return NotFound();

        Form = SkillForm.From(Skill);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Skill = await session.LoadAsync<Skill>(Id, ct);
        if (Skill is null) return NotFound();

        if (!ModelState.IsValid) return Page();

        var newName = Form.Name.Trim();
        var changed = false;

        if (!string.Equals(Skill.Name, newName, StringComparison.Ordinal))
        {
            session.Events.Append(Id, new SkillRenamed(Id, newName));
            changed = true;
        }

        if (Skill.Category != Form.Category)
        {
            session.Events.Append(Id, new SkillCategoryChanged(Id, Form.Category));
            changed = true;
        }

        if (changed)
            await session.SaveChangesAsync(ct);

        Skill = await session.LoadAsync<Skill>(Id, ct);
        Form = SkillForm.From(Skill!);
        Saved = true;
        return Page();
    }

    public async Task<IActionResult> OnPostRetireAsync(CancellationToken ct)
    {
        var skill = await session.LoadAsync<Skill>(Id, ct);
        if (skill is null) return NotFound();
        if (skill.Retired) return RedirectToPage("Index");

        session.Events.Append(Id, new SkillRetired(Id));
        await session.SaveChangesAsync(ct);
        return RedirectToPage("Index");
    }

    public sealed class SkillForm
    {
        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public SkillCategory Category { get; set; }

        public static SkillForm From(Skill s) => new()
        {
            Name = s.Name,
            Category = s.Category,
        };
    }
}
