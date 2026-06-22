namespace LupiraWeb.Server.Data.Repositories;

/// <summary>The owner's profile composed with their identity email, flattened for the public résumé.</summary>
public sealed record OwnerInfo(
    Guid Id,
    string FullName,
    string Email,
    string? Tagline,
    string? Bio,
    string? Location,
    string? GithubUrl,
    string? LinkedInUrl,
    string? WebsiteUrl);

public interface IMyInfoRepository
{
    Task<OwnerInfo?> GetAsync(CancellationToken ct);
}
