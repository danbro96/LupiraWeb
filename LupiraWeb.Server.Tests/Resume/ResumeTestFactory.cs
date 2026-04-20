using LupiraWeb.Server.Domain;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace LupiraWeb.Server.Tests.Resume;

public class ResumeTestFactory : WebApplicationFactory<Program>
{
    public static readonly Guid SeededEngagementId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid SeededProjectId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid SeededSkillId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid SeededTitleId = Guid.Parse("40000000-0000-0000-0000-000000000001");

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public ResumeTestFactory()
    {
        _postgres.StartAsync().GetAwaiter().GetResult();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var connectionString = _postgres.GetConnectionString();

        builder.ConfigureAppConfiguration(cfg =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AppDb"] = connectionString,
            });
        });

        builder.ConfigureServices(services =>
        {
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            SeedAsync(store).GetAwaiter().GetResult();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _postgres.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    private static async Task SeedAsync(IDocumentStore store)
    {
        await using var session = store.LightweightSession();

        var existing = await session.LoadAsync<MyInfo>(MyInfo.SingletonId);
        if (existing is not null) return;

        session.Store(new MyInfo
        {
            Id = MyInfo.SingletonId,
            FullName = "Test User",
            Email = "test@example.com",
            Tagline = "Tester",
        });

        var start = new DateOnly(2023, 1, 1);

        session.Events.StartStream<Skill>(SeededSkillId,
            new SkillRegistered(SeededSkillId, "C#", SkillCategory.Language, null, null),
            new SkillLearned(SeededSkillId, start, Maturity.Working,
                SkillEdgeContext.InEngagement(SeededEngagementId), null, null));

        session.Events.StartStream<Engagement>(SeededEngagementId,
            new EngagementStarted(SeededEngagementId, EngagementKind.Employment, "Strivo", start, null, null),
            new TitleAssumed(SeededEngagementId, SeededTitleId, "Consultant", start),
            new EngagementSkillAttached(SeededEngagementId, SeededSkillId, start));

        session.Events.StartStream<Project>(SeededProjectId,
            new ProjectStarted(SeededProjectId, ProjectKind.Professional, "LupiraWeb", null, SeededEngagementId, null, start),
            new ProjectSkillAttached(SeededProjectId, SeededSkillId, start));

        session.Events.Append(SeededSkillId,
            new SkillApplied(SeededSkillId, start, Intensity.Regular,
                SkillEdgeContext.InProject(SeededProjectId), null, null));

        await session.SaveChangesAsync();
    }
}
