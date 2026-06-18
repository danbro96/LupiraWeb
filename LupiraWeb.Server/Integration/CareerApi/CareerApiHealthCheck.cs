using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LupiraWeb.Server.Integration.CareerApi;

/// <summary>
/// Readiness check: a cheap GET to CareerApi's anonymous <c>/livez</c> proving the upstream is reachable.
/// Uses a dedicated, unauthenticated client (no auth handler, short timeout) — readiness reflects
/// reachability, not token issuance. LupiraWeb can serve nothing without CareerApi, so a failure here
/// correctly marks the instance not-ready (while <c>/livez</c> stays dependency-free).
/// </summary>
internal sealed class CareerApiHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public const string HealthClientName = "careerApi-health";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(HealthClientName);
            using var response = await client.GetAsync("/livez", cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("CareerApi reachable.")
                : HealthCheckResult.Unhealthy($"CareerApi /livez returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("CareerApi unreachable.", ex);
        }
    }
}
