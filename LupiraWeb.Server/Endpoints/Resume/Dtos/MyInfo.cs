using MyInfoDocument = LupiraWeb.Domain.MyInfo;

namespace LupiraWeb.Server.Endpoints.Resume.Dtos;

public sealed record MyInfo
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

    public static MyInfo From(MyInfoDocument m) => new()
    {
        Id = m.Id,
        FullName = m.FullName,
        Email = m.Email,
        Tagline = m.Tagline,
        Bio = m.Bio,
        Location = m.Location,
        GithubUrl = m.GithubUrl,
        LinkedInUrl = m.LinkedInUrl,
        WebsiteUrl = m.WebsiteUrl,
    };
}
