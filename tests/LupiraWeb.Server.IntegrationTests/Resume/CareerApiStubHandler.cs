using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LupiraWeb.Server.Contracts;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Tests.Resume;

/// <summary>
/// In-process stub for LupiraCareerApi. Replaces the typed client's network handler so the full path —
/// endpoint → handler → repository → <c>CareerApiClient</c> → auth handler → here — is exercised without a
/// live CareerApi or a database. Serves the same seeded data the old Marten seed produced, so the existing
/// integration assertions hold. Enums are emitted as numbers (web defaults), matching CareerApi's real output.
/// </summary>
internal sealed class CareerApiStubHandler : HttpMessageHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
    private static readonly DateOnly Start = new(2023, 1, 1);

    public static readonly Guid OwnerPrincipalId = Guid.Parse("90000000-0000-0000-0000-000000000001");
    public static readonly Guid OrganizationId = Guid.Parse("80000000-0000-0000-0000-000000000001");

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var segments = request.RequestUri!.AbsolutePath.Trim('/').Split('/');
        return Task.FromResult(Route(segments));
    }

    // The public, handle-addressed surface (/public/{handle}/…); the handle segment is ignored by the stub.
    private static HttpResponseMessage Route(string[] segments) => segments switch
    {
        ["livez"] => Empty(),
        ["public", _, "profile"] => Json(Profile),
        ["public", _, "engagements"] => Json(new[] { Engagement }),
        ["public", _, "engagements", var id] => MatchOr404(id, ResumeTestFactory.SeededEngagementId, Engagement),
        ["public", _, "projects"] => Json(new[] { Project }),
        ["public", _, "projects", var id] => MatchOr404(id, ResumeTestFactory.SeededProjectId, Project),
        ["public", _, "skills"] => Json(new[] { Skill }),
        ["public", _, "skills", var id] => MatchOr404(id, ResumeTestFactory.SeededSkillId, Skill),
        ["public", _, "skills", var id, "timeline"] => MatchOr404(id, ResumeTestFactory.SeededSkillId, Timeline),
        ["public", _, "skills", var id, "maturity"] => MatchOr404(id, ResumeTestFactory.SeededSkillId, Maturity),
        ["public", _, "experience"] => Json(Experience),
        ["public", _, "media"] => Json(Array.Empty<CareerMediaDto>()),
        ["public", _, "artifacts"] => Json(Array.Empty<CareerArtifactDto>()),
        _ => new HttpResponseMessage(HttpStatusCode.NotFound),
    };

    private static HttpResponseMessage MatchOr404<T>(string routeId, Guid seededId, T value) =>
        Guid.TryParse(routeId, out var id) && id == seededId
            ? Json(value)
            : new HttpResponseMessage(HttpStatusCode.NotFound);

    private static HttpResponseMessage Empty() => new(HttpStatusCode.OK);

    private static HttpResponseMessage Json<T>(T value) =>
        new(HttpStatusCode.OK) { Content = JsonContent.Create(value, options: JsonOpts) };

    private static CareerProfileDto Profile =>
        new(OwnerPrincipalId, "Test User", "Tester", null, null, null, null, null);

    private static CareerEngagementDto Engagement => new(
        ResumeTestFactory.SeededEngagementId,
        EngagementKind.Employment,
        OrganizationId,
        "Strivo",
        Start,
        null,
        null,
        null,
        "Consultant",
        [new CareerTitleEpochDto(ResumeTestFactory.SeededTitleId, "Consultant", Start, null)],
        [ResumeTestFactory.SeededSkillId]);

    private static CareerProjectDto Project => new(
        ResumeTestFactory.SeededProjectId,
        ProjectKind.Professional,
        "LupiraWeb",
        null,
        null,
        ResumeTestFactory.SeededEngagementId,
        Start,
        null,
        null,
        ProjectStatus.Active,
        [ResumeTestFactory.SeededSkillId]);

    private static CareerSkillDto Skill => new(
        ResumeTestFactory.SeededSkillId,
        "C#",
        SkillCategory.Language,
        [],
        null,
        false,
        Start,
        LupiraWeb.Server.Contracts.Maturity.Working);

    private static CareerSkillTimelineDto Timeline => new(
        ResumeTestFactory.SeededSkillId,
        OwnerPrincipalId,
        "C#",
        [
            new CareerSkillTimelineEntryDto("Learned", Start, SkillContextKind.InEngagement,
                ResumeTestFactory.SeededEngagementId, null, null, LupiraWeb.Server.Contracts.Maturity.Working, null, null),
            new CareerSkillTimelineEntryDto("Applied", Start, SkillContextKind.InProject,
                ResumeTestFactory.SeededProjectId, null, Intensity.Regular, null, null, null),
        ]);

    private static CareerSkillMaturityDto Maturity => new(
        ResumeTestFactory.SeededSkillId,
        OwnerPrincipalId,
        LupiraWeb.Server.Contracts.Maturity.Working,
        [new CareerSkillMaturityPointDto(Start, LupiraWeb.Server.Contracts.Maturity.Working, "Learned")]);

    private static CareerExperienceItemDto[] Experience =>
    [
        new(ExperienceKind.Engagement, ResumeTestFactory.SeededEngagementId, "Strivo", Start, null,
            OrganizationId, "Strivo", null, [ResumeTestFactory.SeededSkillId]),
        new(ExperienceKind.Project, ResumeTestFactory.SeededProjectId, "LupiraWeb", Start, null,
            null, null, null, [ResumeTestFactory.SeededSkillId]),
    ];
}
