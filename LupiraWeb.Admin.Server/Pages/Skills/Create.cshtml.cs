using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LupiraWeb.Admin.Server.Pages.Skills;

public sealed class CreateModel(IDocumentSession session) : PageModel
{
    [BindProperty]
    public SkillForm Form { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var skillId = Guid.CreateVersion7();
        session.Events.StartStream<Skill>(skillId,
            new SkillRegistered(skillId, Form.Name.Trim(), Form.Category, Aliases: null, ParentSkillId: null));
        await session.SaveChangesAsync(ct);

        return RedirectToPage("Index");
    }

    public sealed class SkillForm
    {
        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public SkillCategory Category { get; set; } = SkillCategory.Tool;
    }
}
