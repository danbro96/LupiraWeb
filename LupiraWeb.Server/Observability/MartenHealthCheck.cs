using Marten;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using LupiraWeb.Domain;

namespace LupiraWeb.Server.Observability;

public sealed class MartenHealthCheck(IDocumentStore store) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            await using var session = store.QuerySession();
            await session.Query<MyInfo>().AnyAsync(ct);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
