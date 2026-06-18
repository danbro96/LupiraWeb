using System.Net.Http.Headers;

namespace LupiraWeb.Server.Integration.CareerApi.Auth;

/// <summary>
/// Development/Testing provider: no bearer token; instead presents CareerApi's dev <c>X-Dev-User</c>
/// header set to the owner email (<c>CareerApi:DevUser</c>), which CareerApi resolves to the owner principal.
/// </summary>
internal sealed class DevUserTokenProvider(IConfiguration configuration) : ICareerApiTokenProvider
{
    public Task<AuthenticationHeaderValue?> GetAuthorizationAsync(CancellationToken ct) =>
        Task.FromResult<AuthenticationHeaderValue?>(null);

    public string? DevUserEmail => configuration["CareerApi:DevUser"];
}
