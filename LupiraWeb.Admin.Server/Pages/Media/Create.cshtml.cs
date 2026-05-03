using LupiraWeb.Domain;
using LupiraWeb.Domain.Infrastructure.BlobStorage;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace LupiraWeb.Admin.Server.Pages.Media;

public sealed class CreateModel(IDocumentSession session, IBlobStorage blobStorage) : PageModel
{
    [BindProperty]
    public IFormFile? File { get; set; }

    [BindProperty]
    [Required, StringLength(500)]
    public string AltText { get; set; } = string.Empty;

    [BindProperty]
    [StringLength(500)]
    public string? Caption { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (File is null || File.Length == 0)
        {
            ModelState.AddModelError(nameof(File), "Please choose a non-empty file.");
            return Page();
        }
        if (!ModelState.IsValid)
            return Page();

        var mediaId = Guid.CreateVersion7();
        await using var stream = File.OpenReadStream();
        var blobRef = await blobStorage.UploadAsync(stream, File.ContentType, ct);

        session.Events.StartStream<MediaAsset>(mediaId,
            new MediaRegistered(
                mediaId,
                blobRef,
                File.ContentType,
                Width: null,
                Height: null,
                AltText.Trim(),
                NullIfBlank(Caption),
                DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);

        return RedirectToPage("Edit", new { id = mediaId });
    }

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
