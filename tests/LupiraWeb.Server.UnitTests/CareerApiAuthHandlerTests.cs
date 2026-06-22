using System.Net;
using System.Net.Http.Headers;
using LupiraWeb.Server.Integration.CareerApi.Auth;
using Xunit;

namespace LupiraWeb.Server.Tests.Integration;

public class CareerApiAuthHandlerTests
{
    [Fact]
    public async Task Attaches_X_Dev_User_header_in_dev_mode()
    {
        var request = await SendThroughAsync(new FakeProvider(authorization: null, devUser: "owner@example.com"));

        Assert.True(request.Headers.Contains("X-Dev-User"));
        Assert.Equal("owner@example.com", request.Headers.GetValues("X-Dev-User").Single());
        Assert.Null(request.Headers.Authorization);
    }

    [Fact]
    public async Task Attaches_bearer_token_in_prod_mode()
    {
        var request = await SendThroughAsync(
            new FakeProvider(new AuthenticationHeaderValue("Bearer", "the-token"), devUser: null));

        Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
        Assert.Equal("the-token", request.Headers.Authorization.Parameter);
        Assert.False(request.Headers.Contains("X-Dev-User"));
    }

    [Fact]
    public async Task Attaches_nothing_when_provider_supplies_neither()
    {
        var request = await SendThroughAsync(new FakeProvider(authorization: null, devUser: null));

        Assert.Null(request.Headers.Authorization);
        Assert.False(request.Headers.Contains("X-Dev-User"));
    }

    private static async Task<HttpRequestMessage> SendThroughAsync(ICareerApiTokenProvider provider)
    {
        var capture = new CapturingHandler();
        var handler = new CareerApiAuthHandler(provider) { InnerHandler = capture };
        using var invoker = new HttpMessageInvoker(handler);
        await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "http://career.test/api/me"), CancellationToken.None);
        return capture.Captured!;
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Captured { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Captured = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class FakeProvider(AuthenticationHeaderValue? authorization, string? devUser)
        : ICareerApiTokenProvider
    {
        public Task<AuthenticationHeaderValue?> GetAuthorizationAsync(CancellationToken ct) =>
            Task.FromResult(authorization);

        public string? DevUserEmail => devUser;
    }
}
