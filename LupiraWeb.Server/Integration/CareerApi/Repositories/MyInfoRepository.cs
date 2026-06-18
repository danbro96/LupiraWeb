using LupiraWeb.Domain;
using LupiraWeb.Server.Data.Repositories;

namespace LupiraWeb.Server.Integration.CareerApi.Repositories;

/// <summary>Composes the owner's profile (CareerApi <c>/api/profile</c>) with their identity
/// (<c>/api/me</c>, for the email) into the public <see cref="MyInfo"/> singleton shape.</summary>
internal sealed class MyInfoRepository(ICareerApiClient client) : IMyInfoRepository
{
    public async Task<MyInfo?> GetAsync(CancellationToken ct)
    {
        var profile = await client.GetProfileAsync(ct);
        if (profile is null)
            return null;

        var me = await client.GetMeAsync(ct);

        return new MyInfo
        {
            Id = profile.OwnerPrincipalId,
            FullName = profile.FullName,
            Email = me?.Email ?? "",
            Tagline = profile.Tagline,
            Bio = profile.Bio,
            Location = profile.Location,
            GithubUrl = profile.GithubUrl,
            LinkedInUrl = profile.LinkedInUrl,
            WebsiteUrl = profile.WebsiteUrl,
        };
    }
}
