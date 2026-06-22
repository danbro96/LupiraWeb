using LupiraWeb.Server.Data.Repositories;

namespace LupiraWeb.Server.Integration.CareerApi.Repositories;

/// <summary>Composes the owner's profile (CareerApi <c>/profile</c>) with their identity
/// (<c>/me</c>, for the email) into the flat <see cref="OwnerInfo"/> the résumé exposes.</summary>
internal sealed class MyInfoRepository(ICareerApiClient client) : IMyInfoRepository
{
    public async Task<OwnerInfo?> GetAsync(CancellationToken ct)
    {
        var profile = await client.GetProfileAsync(ct);
        if (profile is null)
            return null;

        var me = await client.GetMeAsync(ct);

        return new OwnerInfo(
            profile.OwnerPrincipalId,
            profile.FullName,
            me?.Email ?? "",
            profile.Tagline,
            profile.Bio,
            profile.Location,
            profile.GithubUrl,
            profile.LinkedInUrl,
            profile.WebsiteUrl);
    }
}
