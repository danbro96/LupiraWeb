using System.Net.Http.Headers;

namespace LupiraWeb.Server.Integration.CareerApi.Auth;

/// <summary>
/// Supplies the credential LupiraWeb presents to LupiraCareerApi. This is the single swappable seam for
/// how the public site authenticates as the portfolio owner's principal: development uses the
/// <c>X-Dev-User</c> header; production uses a bearer token.
/// </summary>
public interface ICareerApiTokenProvider
{
    /// <summary>Bearer (or other) Authorization header to attach, or <c>null</c> for none.</summary>
    Task<AuthenticationHeaderValue?> GetAuthorizationAsync(CancellationToken ct);

    /// <summary>Owner email for CareerApi's dev <c>X-Dev-User</c> header, or <c>null</c> outside dev.</summary>
    string? DevUserEmail { get; }
}
