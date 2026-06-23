using System.Net.Http.Headers;

namespace LupiraWeb.Server.Integration.CareerApi.Auth;

/// <summary>
/// Supplies the credential LupiraWeb presents to LupiraCareerApi's public read surface. The single swappable
/// seam for how the site authenticates: development uses the <c>X-Dev-User</c> header; production mints a
/// machine (client-credentials) bearer token and refreshes it. The token is a gate, not an owner identity —
/// the owner is selected by the public handle in the URL.
/// </summary>
public interface ICareerApiTokenProvider
{
    /// <summary>Bearer (or other) Authorization header to attach, or <c>null</c> for none.</summary>
    Task<AuthenticationHeaderValue?> GetAuthorizationAsync(CancellationToken ct);

    /// <summary>Owner email for CareerApi's dev <c>X-Dev-User</c> header, or <c>null</c> outside dev.</summary>
    string? DevUserEmail { get; }

    /// <summary>Drops any cached credential so the next call re-acquires it — lets the auth handler recover
    /// from a 401 (e.g. a revoked token). A no-op for providers that hold no cache.</summary>
    void Invalidate();
}
