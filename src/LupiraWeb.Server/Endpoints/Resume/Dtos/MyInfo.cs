using LupiraWeb.Server.Data.Repositories;

namespace LupiraWeb.Server.Endpoints.Resume.Dtos;

public sealed class MyInfo
{
    public required Guid Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public string? Tagline { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? GithubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? WebsiteUrl { get; set; }

    public static MyInfo From(OwnerInfo o) => new()
    {
        Id = o.Id,
        FullName = o.FullName,
        Email = o.Email,
        Tagline = o.Tagline,
        Bio = o.Bio,
        Location = o.Location,
        GithubUrl = o.GithubUrl,
        LinkedInUrl = o.LinkedInUrl,
        WebsiteUrl = o.WebsiteUrl,
    };
}
