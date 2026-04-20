using System.Text.Json;
using System.Text.Json.Serialization;
using LupiraWeb.Domain;
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

        var learned = new HashSet<Guid>();

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

            if (e.EndDate is DateOnly endDate)
            {
                session.Events.Append(engagementId,
                    new TitleRetired(engagementId, titleId, endDate),
                    new EngagementEnded(engagementId, endDate, Reason: null));
            }

            var engagementSkillIds = new List<Guid>();
            foreach (var skillName in e.Skills)
            {
                if (!skillIdsByName.TryGetValue(skillName, out var skillId))
                    continue;
                engagementSkillIds.Add(skillId);
                session.Events.Append(engagementId,
                    new EngagementSkillAttached(engagementId, skillId, e.StartDate));

                EmitSkillEdgeEvent(session, skillId, e.StartDate,
                    SkillEdgeContext.InEngagement(engagementId), learned);
            }

            EmitPairCombinations(session, engagementSkillIds, e.StartDate,
                ctx => SkillEdgeContext.InEngagement(engagementId));
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
            var occurredOn = p.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            session.Events.StartStream<Project>(projectId,
                new ProjectStarted(
                    projectId,
                    kind,
                    p.Title,
                    p.Description,
                    engagementId,
                    p.Url,
                    p.StartDate));

            if (p.EndDate is DateOnly projectEnd)
            {
                session.Events.Append(projectId,
                    new ProjectShipped(projectId, projectEnd, Outcome: null));
            }

            var projectSkillIds = new List<Guid>();
            foreach (var skillName in p.Skills)
            {
                if (!skillIdsByName.TryGetValue(skillName, out var skillId))
                    continue;
                projectSkillIds.Add(skillId);
                session.Events.Append(projectId,
                    new ProjectSkillAttached(projectId, skillId, p.StartDate));

                EmitSkillEdgeEvent(session, skillId, occurredOn,
                    SkillEdgeContext.InProject(projectId), learned);
            }

            EmitPairCombinations(session, projectSkillIds, occurredOn,
                ctx => SkillEdgeContext.InProject(projectId));
        }

        await session.SaveChangesAsync();
    }

    private static void EmitSkillEdgeEvent(
        IDocumentSession session,
        Guid skillId,
        DateOnly occurredOn,
        SkillEdgeContext context,
        HashSet<Guid> learned)
    {
        if (learned.Add(skillId))
        {
            session.Events.Append(skillId,
                new SkillLearned(skillId, occurredOn, Maturity.Working, context, Evidence: null, Location: null));
        }
        else
        {
            session.Events.Append(skillId,
                new SkillApplied(skillId, occurredOn, Intensity.Regular, context, Evidence: null, Location: null));
        }
    }

    private static void EmitPairCombinations(
        IDocumentSession session,
        IReadOnlyList<Guid> skillIds,
        DateOnly occurredOn,
        Func<object, SkillEdgeContext> contextFactory)
    {
        for (var i = 0; i < skillIds.Count; i++)
        {
            for (var j = i + 1; j < skillIds.Count; j++)
            {
                var a = skillIds[i];
                var b = skillIds[j];
                var (primary, secondary) = a.CompareTo(b) < 0 ? (a, b) : (b, a);

                session.Events.Append(primary,
                    new SkillsCombined(primary, secondary, occurredOn, IsPrimary: true,
                        contextFactory(null!), Evidence: null));
                session.Events.Append(secondary,
                    new SkillsCombined(secondary, primary, occurredOn, IsPrimary: false,
                        contextFactory(null!), Evidence: null));
            }
        }
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
