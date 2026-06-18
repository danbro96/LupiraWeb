using System.Net.Http.Headers;

namespace LupiraWeb.Server.Integration.CareerApi.Auth;

/// <summary>
/// Production provider: presents a pre-issued owner bearer token (<c>CareerApi:Auth:Token</c>) that resolves
/// to the portfolio owner's principal in CareerApi (which keys principals by the token's <c>sub</c>/<c>email</c>).
/// <para>
/// This is the single swap point for a full Authentik client-credentials / token-exchange flow later: replace
/// the static-token read here with a cached, auto-refreshing token acquisition. Nothing else in the integration
/// is aware of how the credential is obtained. (See plan risk R1.)
/// </para>
/// </summary>
internal sealed class OwnerBearerTokenProvider(IConfiguration configuration) : ICareerApiTokenProvider
{
    public Task<AuthenticationHeaderValue?> GetAuthorizationAsync(CancellationToken ct)
    {
        var token = configuration["CareerApi:Auth:Token"];
        var header = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
        return Task.FromResult(header);
    }

    public string? DevUserEmail => null;
}
