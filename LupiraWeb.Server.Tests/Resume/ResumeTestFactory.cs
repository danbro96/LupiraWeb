using LupiraWeb.Server.Integration.CareerApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LupiraWeb.Server.Tests.Resume;

/// <summary>
/// Boots the API against an in-process CareerApi stub (see <see cref="CareerApiStubHandler"/>) instead of a
/// real CareerApi or Postgres. The stub's primary message handler is swapped into both the typed client and
/// the readiness probe client, so endpoints/handlers/mapping run end-to-end over the real HTTP pipeline.
/// </summary>
public class ResumeTestFactory : WebApplicationFactory<Program>
{
    public static readonly Guid SeededEngagementId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid SeededProjectId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid SeededSkillId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid SeededTitleId = Guid.Parse("40000000-0000-0000-0000-000000000001");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(cfg =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CareerApi:BaseUrl"] = "http://career.test",
                ["CareerApi:DevUser"] = "test@example.com",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Route both CareerApi clients (the typed client and the readiness probe) at the in-process stub.
            services.AddHttpClient<ICareerApiClient, CareerApiClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new CareerApiStubHandler());

            services.AddHttpClient(CareerApiHealthCheck.HealthClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new CareerApiStubHandler());
        });
    }
}
