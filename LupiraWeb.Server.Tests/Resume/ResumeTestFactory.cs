using LupiraWeb.Server.Data;
using LupiraWeb.Server.Data.Entities;
using LupiraWeb.Server.Domain;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace LupiraWeb.Server.Tests.Resume;

public class ResumeTestFactory : WebApplicationFactory<Program>
{
    public static readonly Guid SeededEmploymentId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid SeededProjectId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid SeededSkillId = Guid.Parse("30000000-0000-0000-0000-000000000001");

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
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

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            SeedFixture(db, store).GetAwaiter().GetResult();
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

    private static async Task SeedFixture(AppDbContext db, IDocumentStore store)
    {
        if (db.Employments.Any()) return;

        var now = DateTimeOffset.UtcNow;

        await using (var session = store.LightweightSession())
        {
            session.Store(new MyInfo
            {
                Id = MyInfo.SingletonId,
                FullName = "Test User",
                Email = "test@example.com",
                Tagline = "Tester",
            });
            await session.SaveChangesAsync();
        }

        var skill = new SkillEntity
        {
            Id = SeededSkillId,
            Name = "C#",
            Category = SkillCategory.Language,
        };
        db.Skills.Add(skill);

        var employment = new EmploymentEntity
        {
            Id = SeededEmploymentId,
            Company = "Strivo",
            Title = "Consultant",
            StartDate = new DateOnly(2023, 1, 1),
        };
        db.Employments.Add(employment);
        db.EmploymentSkills.Add(new EmploymentSkillEntity
        {
            EmploymentId = employment.Id,
            SkillId = skill.Id,
            GainedAt = now,
        });

        var project = new ProjectEntity
        {
            Id = SeededProjectId,
            Title = "LupiraWeb",
            EmploymentId = employment.Id,
        };
        db.Projects.Add(project);
        db.ProjectSkills.Add(new ProjectSkillEntity
        {
            ProjectId = project.Id,
            SkillId = skill.Id,
            GainedAt = now,
        });

        await db.SaveChangesAsync();
    }
}
