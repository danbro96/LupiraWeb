using System.Net;
using System.Text;
using LupiraWeb.Server.Integration.CareerApi.Auth;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LupiraWeb.Server.Tests.Integration;

public class ClientCredentialsTokenProviderTests
{
    [Fact]
    public async Task Mints_once_then_serves_cached_token_until_invalidated()
    {
        var stub = new TokenEndpointStub();
        var provider = new ClientCredentialsTokenProvider(new StubHttpClientFactory(stub), Config());

        var first = await provider.GetAuthorizationAsync(CancellationToken.None);
        Assert.Equal("Bearer", first!.Scheme);
        Assert.Equal("tok-1", first.Parameter);
        Assert.Equal(1, stub.Calls);

        // Within validity: served from cache, no second mint.
        var cached = await provider.GetAuthorizationAsync(CancellationToken.None);
        Assert.Equal("tok-1", cached!.Parameter);
        Assert.Equal(1, stub.Calls);

        // After invalidation (e.g. a 401): re-mints.
        provider.Invalidate();
        var reminted = await provider.GetAuthorizationAsync(CancellationToken.None);
        Assert.Equal("tok-2", reminted!.Parameter);
        Assert.Equal(2, stub.Calls);
    }

    [Fact]
    public async Task Posts_the_authentik_service_account_client_credentials_form()
    {
        var stub = new TokenEndpointStub();
        var provider = new ClientCredentialsTokenProvider(new StubHttpClientFactory(stub), Config());

        await provider.GetAuthorizationAsync(CancellationToken.None);

        Assert.Contains("grant_type=client_credentials", stub.LastBody);
        Assert.Contains("client_id=lupira-web", stub.LastBody);
        Assert.Contains("username=lupira-web-svc", stub.LastBody);
        Assert.Contains("password=svc-token", stub.LastBody);
        Assert.Contains("lupira-career-aud", stub.LastBody);
    }

    private static IConfiguration Config() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["CareerApi:Auth:TokenEndpoint"] = "https://auth.test/application/o/token/",
            ["CareerApi:Auth:ClientId"] = "lupira-web",
            ["CareerApi:Auth:ClientSecret"] = "secret",
            ["CareerApi:Auth:ServiceAccountUsername"] = "lupira-web-svc",
            ["CareerApi:Auth:ServiceAccountToken"] = "svc-token",
            ["CareerApi:Auth:Scope"] = "openid lupira-career-aud",
        }).Build();

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class TokenEndpointStub : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public string LastBody { get; private set; } = "";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls++;
            LastBody = request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct);
            var json = $$"""{"access_token":"tok-{{Calls}}","expires_in":300}""";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
        }
    }
}
