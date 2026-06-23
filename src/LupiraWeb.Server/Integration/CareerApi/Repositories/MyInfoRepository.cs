using LupiraWeb.Server.Data.Repositories;

namespace LupiraWeb.Server.Integration.CareerApi.Repositories;

/// <summary>Maps the owner's public profile (CareerApi <c>/public/{handle}/profile</c>) to the flat
/// <see cref="OwnerInfo"/> the résumé exposes. Email is intentionally absent from the public surface (it is
/// owner-scoped identity, not published profile data), so it is left empty here.</summary>
internal sealed class MyInfoRepository(ICareerApiClient client) : IMyInfoRepository
{
    public async Task<OwnerInfo?> GetAsync(CancellationToken ct)
    {
        var profile = await client.GetProfileAsync(ct);
        if (profile is null)
            return null;

        return new OwnerInfo(
            profile.OwnerPrincipalId,
            profile.FullName,
            "",
            profile.Tagline,
            profile.Bio,
            profile.Location,
            profile.GithubUrl,
            profile.LinkedInUrl,
            profile.WebsiteUrl);
    }
}
