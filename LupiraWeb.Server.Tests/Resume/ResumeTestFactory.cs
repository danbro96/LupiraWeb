using LupiraWeb.Server.Data;
using LupiraWeb.Server.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LupiraWeb.Server.Tests.Resume;

public class ResumeTestFactory : WebApplicationFactory<Program>
{
    public static readonly Guid SeededEmploymentId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid SeededProjectId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid SeededSkillId = Guid.Parse("30000000-0000-0000-0000-000000000001");

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedFixture(db);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }

    private static void SeedFixture(AppDbContext db)
    {
        if (db.MyInfo.Any()) return;

        var now = DateTimeOffset.UtcNow;

        db.MyInfo.Add(new MyInfoEntity
        {
            Id = MyInfoEntity.SingletonId,
            FullName = "Test User",
            Email = "test@example.com",
            Tagline = "Tester",
        });

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

        db.SaveChanges();
    }
}
