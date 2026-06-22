namespace LupiraWeb.Server.Integration.CareerApi.Auth;

/// <summary>
/// Delegating handler that attaches the owner credential supplied by <see cref="ICareerApiTokenProvider"/>
/// to every outgoing CareerApi request. The credential mechanism (dev header vs prod bearer) is decided by
/// the registered provider; this handler is identical across environments.
/// </summary>
internal sealed class CareerApiAuthHandler(ICareerApiTokenProvider tokens) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = await tokens.GetAuthorizationAsync(cancellationToken);
        if (authorization is not null)
            request.Headers.Authorization = authorization;

        var devUser = tokens.DevUserEmail;
        if (!string.IsNullOrEmpty(devUser))
            request.Headers.TryAddWithoutValidation("X-Dev-User", devUser);

        return await base.SendAsync(request, cancellationToken);
    }
}
