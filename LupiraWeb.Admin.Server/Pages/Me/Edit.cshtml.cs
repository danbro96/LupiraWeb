using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LupiraWeb.Admin.Server.Pages.Me;

public sealed class EditModel(IDocumentSession session) : PageModel
{
    [BindProperty]
    public MyInfoForm Form { get; set; } = new();

    public bool Saved { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var existing = await session.LoadAsync<MyInfo>(MyInfo.SingletonId, ct);
        if (existing is not null)
        {
            Form = MyInfoForm.From(existing);
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var myInfo = new MyInfo
        {
            Id = MyInfo.SingletonId,
            FullName = Form.FullName,
            Email = Form.Email,
            Tagline = NullIfBlank(Form.Tagline),
            Bio = NullIfBlank(Form.Bio),
            Location = NullIfBlank(Form.Location),
            GithubUrl = NullIfBlank(Form.GithubUrl),
            LinkedInUrl = NullIfBlank(Form.LinkedInUrl),
            WebsiteUrl = NullIfBlank(Form.WebsiteUrl),
        };

        session.Store(myInfo);
        await session.SaveChangesAsync(ct);

        Saved = true;
        return Page();
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public sealed class MyInfoForm
    {
        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Tagline { get; set; }

        [StringLength(4000)]
        public string? Bio { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [Url, StringLength(500)]
        public string? GithubUrl { get; set; }

        [Url, StringLength(500)]
        public string? LinkedInUrl { get; set; }

        [Url, StringLength(500)]
        public string? WebsiteUrl { get; set; }

        public static MyInfoForm From(MyInfo info) => new()
        {
            FullName = info.FullName,
            Email = info.Email,
            Tagline = info.Tagline,
            Bio = info.Bio,
            Location = info.Location,
            GithubUrl = info.GithubUrl,
            LinkedInUrl = info.LinkedInUrl,
            WebsiteUrl = info.WebsiteUrl,
        };
    }
}
