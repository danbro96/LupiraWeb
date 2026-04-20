using System.Text.Json;
using System.Text.Json.Serialization;
using LupiraWeb.Server.Data.Entities;
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

        EnsureSqliteDirectoryExists(db.Database.GetConnectionString());

        await db.Database.MigrateAsync();
        await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");

        if (await db.MyInfo.AnyAsync())
            return;

        var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "seed.dev.json");
        if (!File.Exists(seedPath))
            return;

        var json = await File.ReadAllTextAsync(seedPath);
        var seed = JsonSerializer.Deserialize<SeedPayload>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse seed.dev.json");

        await SeedAsync(db, seed);
    }

    private static async Task SeedAsync(AppDbContext db, SeedPayload seed)
    {
        var now = DateTimeOffset.UtcNow;
        long sequence = 0;

        var myInfo = new MyInfoEntity
        {
            Id = MyInfoEntity.SingletonId,
            FullName = seed.MyInfo.FullName,
            Email = seed.MyInfo.Email,
            Tagline = seed.MyInfo.Tagline,
            Bio = seed.MyInfo.Bio,
            Location = seed.MyInfo.Location,
            GithubUrl = seed.MyInfo.GithubUrl,
            LinkedInUrl = seed.MyInfo.LinkedInUrl,
            WebsiteUrl = seed.MyInfo.WebsiteUrl,
        };
        db.MyInfo.Add(myInfo);
        db.Events.Add(EventFor(myInfo.Id, "MyInfo", "MyInfoSet",
            new { myInfo.Id, myInfo.FullName, myInfo.Email, myInfo.Tagline, myInfo.Bio,
                  myInfo.Location, myInfo.GithubUrl, myInfo.LinkedInUrl, myInfo.WebsiteUrl },
            now, ++sequence));

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
            db.Events.Add(EventFor(skill.Id, "Skill", "SkillAdded",
                new { skill.Id, skill.Name, skill.Category },
                now, ++sequence));
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
            db.Events.Add(EventFor(employment.Id, "Employment", "EmploymentAdded",
                new { employment.Id, employment.Company, employment.Title,
                      employment.StartDate, employment.EndDate, employment.Summary, employment.Location },
                now, ++sequence));

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
                db.Events.Add(EventFor(
                    employment.Id, "Employment", "SkillTaggedToEmployment",
                    new { EmploymentId = employment.Id, SkillId = skill.Id, GainedAt = DeriveGainedAt(employment.StartDate) },
                    now, ++sequence));
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
            db.Events.Add(EventFor(project.Id, "Project", "ProjectAdded",
                new { project.Id, project.Title, project.Description, project.Url,
                      project.StartDate, project.EndDate, project.EmploymentId },
                now, ++sequence));

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
                db.Events.Add(EventFor(
                    project.Id, "Project", "SkillTaggedToProject",
                    new { ProjectId = project.Id, SkillId = skill.Id, GainedAt = gainedAt },
                    now, ++sequence));
            }
        }

        await db.SaveChangesAsync();
    }

    private static DateTimeOffset DeriveGainedAt(DateOnly? sourceDate) =>
        sourceDate is DateOnly d
            ? new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            : DateTimeOffset.UtcNow;

    private static EventEntity EventFor(
        Guid aggregateId, string aggregateType, string eventType,
        object payload, DateTimeOffset occurredAt, long sequence) => new()
    {
        Id = Guid.CreateVersion7(),
        AggregateId = aggregateId,
        AggregateType = aggregateType,
        EventType = eventType,
        PayloadJson = JsonSerializer.Serialize(
            new { schemaVersion = 1, data = payload },
            JsonOptions),
        OccurredAt = occurredAt,
        Sequence = sequence,
    };

    private static void EnsureSqliteDirectoryExists(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return;
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource)) return;
        if (builder.DataSource.StartsWith(":memory:", StringComparison.OrdinalIgnoreCase)) return;
        var dir = Path.GetDirectoryName(Path.GetFullPath(builder.DataSource));
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
    }

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
