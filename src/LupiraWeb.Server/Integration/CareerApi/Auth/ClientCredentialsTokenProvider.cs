using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace LupiraWeb.Server.Integration.CareerApi.Auth;

/// <summary>
/// Production provider: mints a machine (OAuth2 client-credentials) access token from Authentik and presents it as
/// a bearer to CareerApi's public read surface. Its <c>sub</c> is the <c>lupira-web-svc</c> service account, not
/// the portfolio owner, and it carries <c>aud: lupira-career</c> via the <c>lupira-career-aud</c> scope mapping —
/// the owner is chosen by the public handle in the URL, so this credential is a gate, not an identity.
///
/// Authentik's client-credentials grant uses the service-account flavour: <c>client_id</c>/<c>client_secret</c>
/// plus the service account's <c>username</c> + token as <c>password</c>. The token is cached and refreshed shortly
/// before expiry; <see cref="Invalidate"/> drops the cache so the auth handler can recover from a 401.
/// </summary>
internal sealed class ClientCredentialsTokenProvider(
    IHttpClientFactory httpFactory,
    IConfiguration configuration) : ICareerApiTokenProvider
{
    public const string TokenClientName = "careerApi-token";

    private static readonly TimeSpan RefreshSkew = TimeSpan.FromSeconds(60);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _expiresAt;

    public string? DevUserEmail => null;

    public async Task<AuthenticationHeaderValue?> GetAuthorizationAsync(CancellationToken ct)
    {
        var token = await GetTokenAsync(ct);
        return new AuthenticationHeaderValue("Bearer", token);
    }

    public void Invalidate()
    {
        _gate.Wait();
        try { _accessToken = null; }
        finally { _gate.Release(); }
    }

    private async Task<string> GetTokenAsync(CancellationToken ct)
    {
        if (_accessToken is not null && DateTimeOffset.UtcNow < _expiresAt)
            return _accessToken;

        await _gate.WaitAsync(ct);
        try
        {
            if (_accessToken is not null && DateTimeOffset.UtcNow < _expiresAt)
                return _accessToken;

            var (token, expiresIn) = await MintAsync(ct);
            _accessToken = token;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn) - RefreshSkew;
            return token;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<(string Token, int ExpiresIn)> MintAsync(CancellationToken ct)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = Require("CareerApi:Auth:ClientId"),
            ["client_secret"] = Require("CareerApi:Auth:ClientSecret"),
            ["username"] = Require("CareerApi:Auth:ServiceAccountUsername"),
            ["password"] = Require("CareerApi:Auth:ServiceAccountToken"),
            ["scope"] = configuration["CareerApi:Auth:Scope"] ?? "openid lupira-career-aud",
        });

        var http = httpFactory.CreateClient(TokenClientName);
        using var response = await http.PostAsync(Require("CareerApi:Auth:TokenEndpoint"), form, ct);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
            ?? throw new InvalidOperationException("Authentik token endpoint returned an empty body.");
        return (token.AccessToken, token.ExpiresIn);
    }

    private string Require(string key) =>
        configuration[key] ?? throw new InvalidOperationException($"{key} is required to mint the CareerApi token.");

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
