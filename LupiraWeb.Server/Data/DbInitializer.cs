using System.Text.Json;
using System.Text.Json.Serialization;
using LupiraWeb.Server.Data.Entities;
using LupiraWeb.Server.Domain;
using Marten;
using Microsoft.EntityFrameworkCore;

namespace LupiraWeb.Server.Data;

public static class DbInitializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();

        await db.Database.EnsureCreatedAsync();

        var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "seed.dev.json");
        if (!File.Exists(seedPath))
            return;

        var json = await File.ReadAllTextAsync(seedPath);
        var seed = JsonSerializer.Deserialize<SeedPayload>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse seed.dev.json");

        await SeedMyInfoAsync(store, seed.MyInfo);
        await SeedRelationalAsync(db, seed);
    }

    private static async Task SeedMyInfoAsync(IDocumentStore store, SeedMyInfo seed)
    {
        await using var session = store.LightweightSession();
        var existing = await session.LoadAsync<MyInfo>(MyInfo.SingletonId);
        if (existing is not null) return;

        session.Store(new MyInfo
        {
            Id = MyInfo.SingletonId,
            FullName = seed.FullName,
            Email = seed.Email,
            Tagline = seed.Tagline,
            Bio = seed.Bio,
            Location = seed.Location,
            GithubUrl = seed.GithubUrl,
            LinkedInUrl = seed.LinkedInUrl,
            WebsiteUrl = seed.WebsiteUrl,
        });
        await session.SaveChangesAsync();
    }

    private static async Task SeedRelationalAsync(AppDbContext db, SeedPayload seed)
    {
        if (db.Skills.Any()) return;

        var skillsByName = new Dictionary<string, SkillEntity>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in seed.Skills)
        {
            var skill = new SkillEntity
            {
                Id = Guid.CreateVersion7(),
                Name = s.Name,
                Category = s.Category,
            };
            skillsByName[s.Name] = skill;
            db.Skills.Add(skill);
        }

        var employmentsByCompany = new Dictionary<string, EmploymentEntity>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in seed.Employments)
        {
            var employment = new EmploymentEntity
            {
                Id = Guid.CreateVersion7(),
                Company = e.Company,
                Title = e.Title,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Summary = e.Summary,
                Location = e.Location,
            };
            employmentsByCompany[e.Company] = employment;
            db.Employments.Add(employment);

            foreach (var skillName in e.Skills)
            {
                if (!skillsByName.TryGetValue(skillName, out var skill))
                    continue;
                db.EmploymentSkills.Add(new EmploymentSkillEntity
                {
                    EmploymentId = employment.Id,
                    SkillId = skill.Id,
                    GainedAt = DeriveGainedAt(employment.StartDate),
                });
            }
        }

        foreach (var p in seed.Projects)
        {
            Guid? employmentId = null;
            if (p.EmploymentCompany is not null
                && employmentsByCompany.TryGetValue(p.EmploymentCompany, out var emp))
            {
                employmentId = emp.Id;
            }

            var project = new ProjectEntity
            {
                Id = Guid.CreateVersion7(),
                Title = p.Title,
                Description = p.Description,
                Url = p.Url,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                EmploymentId = employmentId,
            };
            db.Projects.Add(project);

            var gainedAt = DeriveGainedAt(p.StartDate);
            foreach (var skillName in p.Skills)
            {
                if (!skillsByName.TryGetValue(skillName, out var skill))
                    continue;
                db.ProjectSkills.Add(new ProjectSkillEntity
                {
                    ProjectId = project.Id,
                    SkillId = skill.Id,
                    GainedAt = gainedAt,
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static DateTimeOffset DeriveGainedAt(DateOnly? sourceDate) =>
        sourceDate is DateOnly d
            ? new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            : DateTimeOffset.UtcNow;

    private sealed record SeedPayload(
        SeedMyInfo MyInfo,
        IReadOnlyList<SeedSkill> Skills,
        IReadOnlyList<SeedEmployment> Employments,
        IReadOnlyList<SeedProject> Projects);

    private sealed record SeedMyInfo(
        string FullName, string Email, string? Tagline, string? Bio,
        string? Location, string? GithubUrl, string? LinkedInUrl, string? WebsiteUrl);

    private sealed record SeedSkill(string Name, SkillCategory Category);

    private sealed record SeedEmployment(
        string Company, string Title, DateOnly StartDate, DateOnly? EndDate,
        string? Location, string? Summary, IReadOnlyList<string> Skills);

    private sealed record SeedProject(
        string Title, string? Description, string? Url,
        DateOnly? StartDate, DateOnly? EndDate,
        string? EmploymentCompany, IReadOnlyList<string> Skills);
}
