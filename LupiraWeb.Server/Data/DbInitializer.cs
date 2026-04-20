using System.Text.Json;
using System.Text.Json.Serialization;
using LupiraWeb.Server.Domain;
using Marten;

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
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();

        var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "seed.dev.json");
        if (!File.Exists(seedPath))
            return;

        var json = await File.ReadAllTextAsync(seedPath);
        var seed = JsonSerializer.Deserialize<SeedPayload>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse seed.dev.json");

        await SeedAsync(store, seed);
    }

    private static async Task SeedAsync(IDocumentStore store, SeedPayload seed)
    {
        await using var session = store.LightweightSession();

        var existingMyInfo = await session.LoadAsync<MyInfo>(MyInfo.SingletonId);
        if (existingMyInfo is not null) return;

        session.Store(new MyInfo
        {
            Id = MyInfo.SingletonId,
            FullName = seed.MyInfo.FullName,
            Email = seed.MyInfo.Email,
            Tagline = seed.MyInfo.Tagline,
            Bio = seed.MyInfo.Bio,
            Location = seed.MyInfo.Location,
            GithubUrl = seed.MyInfo.GithubUrl,
            LinkedInUrl = seed.MyInfo.LinkedInUrl,
            WebsiteUrl = seed.MyInfo.WebsiteUrl,
        });

        var skillIdsByName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in seed.Skills)
        {
            var skillId = Guid.CreateVersion7();
            skillIdsByName[s.Name] = skillId;
            session.Events.StartStream<Skill>(skillId,
                new SkillRegistered(skillId, s.Name, s.Category, null, null));
        }

        var engagementIdsByCompany = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in seed.Employments)
        {
            var engagementId = Guid.CreateVersion7();
            var titleId = Guid.NewGuid();
            engagementIdsByCompany[e.Company] = engagementId;

            session.Events.StartStream<Engagement>(engagementId,
                new EngagementStarted(
                    engagementId,
                    EngagementKind.Employment,
                    e.Company,
                    e.StartDate,
                    Location: null,
                    Summary: e.Summary),
                new TitleAssumed(engagementId, titleId, e.Title, e.StartDate));

            if (e.EndDate is DateOnly end)
            {
                session.Events.Append(engagementId,
                    new TitleRetired(engagementId, titleId, end),
                    new EngagementEnded(engagementId, end, Reason: null));
            }

            foreach (var skillName in e.Skills)
            {
                if (!skillIdsByName.TryGetValue(skillName, out var skillId))
                    continue;
                session.Events.Append(engagementId,
                    new EngagementSkillAttached(engagementId, skillId, e.StartDate));
            }
        }

        foreach (var p in seed.Projects)
        {
            var projectId = Guid.CreateVersion7();
            Guid? engagementId = null;
            if (p.EmploymentCompany is not null
                && engagementIdsByCompany.TryGetValue(p.EmploymentCompany, out var eid))
            {
                engagementId = eid;
            }

            var kind = engagementId.HasValue ? ProjectKind.Professional : ProjectKind.Personal;

            session.Events.StartStream<Project>(projectId,
                new ProjectStarted(
                    projectId,
                    kind,
                    p.Title,
                    p.Description,
                    engagementId,
                    p.Url,
                    p.StartDate));

            if (p.EndDate is DateOnly end)
            {
                session.Events.Append(projectId,
                    new ProjectShipped(projectId, end, Outcome: null));
            }

            foreach (var skillName in p.Skills)
            {
                if (!skillIdsByName.TryGetValue(skillName, out var skillId))
                    continue;
                session.Events.Append(projectId,
                    new ProjectSkillAttached(projectId, skillId, p.StartDate));
            }
        }

        await session.SaveChangesAsync();
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
