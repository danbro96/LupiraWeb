using System.Net;

namespace LupiraWeb.Server.Integration.CareerApi.Auth;

/// <summary>
/// Delegating handler that attaches the credential supplied by <see cref="ICareerApiTokenProvider"/> to every
/// outgoing CareerApi request. The mechanism (dev header vs prod minted bearer) is decided by the registered
/// provider; this handler is identical across environments. On a 401 it invalidates the cached credential and
/// retries once (the reads are bodyless GETs, so the request is safely rebuilt), recovering from a revoked token.
/// </summary>
internal sealed class CareerApiAuthHandler(ICareerApiTokenProvider tokens) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AttachAsync(request, cancellationToken);
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized || request.Content is not null)
            return response;

        // Token may have been revoked before its cached expiry — drop it, re-acquire, retry once.
        response.Dispose();
        tokens.Invalidate();
        var retry = new HttpRequestMessage(request.Method, request.RequestUri);
        await AttachAsync(retry, cancellationToken);
        return await base.SendAsync(retry, cancellationToken);
    }

    private async Task AttachAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var authorization = await tokens.GetAuthorizationAsync(ct);
        if (authorization is not null)
            request.Headers.Authorization = authorization;

        var devUser = tokens.DevUserEmail;
        if (!string.IsNullOrEmpty(devUser))
            request.Headers.TryAddWithoutValidation("X-Dev-User", devUser);
    }
}
