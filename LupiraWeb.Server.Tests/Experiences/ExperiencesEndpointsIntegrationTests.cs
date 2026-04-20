using System.Net;
using System.Net.Http.Json;
using LupiraWeb.Server.Domain;
using LupiraWeb.Server.Endpoints.Experiences.Dtos;
using LupiraWeb.Server.Tests.Resume;
using Xunit;

namespace LupiraWeb.Server.Tests.Experiences;

public class ExperiencesEndpointsIntegrationTests : IClassFixture<ResumeTestFactory>
{
    private readonly ResumeTestFactory _factory;

    public ExperiencesEndpointsIntegrationTests(ResumeTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task List_returns_seeded_engagement_and_project()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/experiences");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<ExperienceDto>>();
        Assert.NotNull(list);
        Assert.Contains(list!, r =>
            r.Kind == ExperienceKind.Engagement && r.Id == ResumeTestFactory.SeededEngagementId);
        Assert.Contains(list!, r =>
            r.Kind == ExperienceKind.Project && r.Id == ResumeTestFactory.SeededProjectId);
    }

    [Fact]
    public async Task List_filters_by_engagement_id()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/experiences?engagementId={ResumeTestFactory.SeededEngagementId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<ExperienceDto>>();
        Assert.NotNull(list);
        Assert.All(list!, r => Assert.Equal(ResumeTestFactory.SeededEngagementId, r.EngagementId));
    }

    [Fact]
    public async Task List_filters_by_skill_id()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/experiences?skillId={ResumeTestFactory.SeededSkillId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<ExperienceDto>>();
        Assert.NotNull(list);
        Assert.All(list!, r => Assert.Contains(ResumeTestFactory.SeededSkillId, r.SkillIds));
    }

    [Fact]
    public async Task List_filters_by_date_range_excludes_earlier_experiences()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/experiences?from=2099-01-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<ExperienceDto>>();
        Assert.NotNull(list);
        Assert.Empty(list!);
    }
}
