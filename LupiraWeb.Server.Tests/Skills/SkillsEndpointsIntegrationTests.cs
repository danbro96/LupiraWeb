using System.Net;
using System.Net.Http.Json;
using LupiraWeb.Domain;
using LupiraWeb.Server.Endpoints.Skills.Dtos;
using LupiraWeb.Server.Tests.Resume;
using Xunit;

namespace LupiraWeb.Server.Tests.Skills;

public class SkillsEndpointsIntegrationTests : IClassFixture<ResumeTestFactory>
{
    private readonly ResumeTestFactory _factory;

    public SkillsEndpointsIntegrationTests(ResumeTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTimeline_returns_200_with_learned_and_applied()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/skills/{ResumeTestFactory.SeededSkillId}/timeline");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var timeline = await response.Content.ReadFromJsonAsync<SkillTimelineResponse>();
        Assert.NotNull(timeline);
        Assert.Equal("C#", timeline!.Name);
        Assert.Contains(timeline.Entries, e => e.Kind == "Learned" && e.ContextKind == SkillContextKind.InEngagement);
        Assert.Contains(timeline.Entries, e => e.Kind == "Applied" && e.ContextKind == SkillContextKind.InProject);
    }

    [Fact]
    public async Task GetTimeline_returns_404_for_unknown_skill()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/skills/{Guid.NewGuid()}/timeline");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRelated_returns_empty_for_single_seeded_skill()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/skills/{ResumeTestFactory.SeededSkillId}/related");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var related = await response.Content.ReadFromJsonAsync<SkillRelatedResponse>();
        Assert.NotNull(related);
        Assert.Empty(related!.Related);
    }

    [Fact]
    public async Task GetRelated_returns_404_for_unknown_skill()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/skills/{Guid.NewGuid()}/related");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMaturity_returns_current_and_trajectory()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/skills/{ResumeTestFactory.SeededSkillId}/maturity");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var maturity = await response.Content.ReadFromJsonAsync<SkillMaturityResponse>();
        Assert.NotNull(maturity);
        Assert.Equal(Maturity.Working, maturity!.Current);
        Assert.Single(maturity.Trajectory);
        Assert.Equal(Maturity.Working, maturity.Trajectory[0].Maturity);
    }

    [Fact]
    public async Task GetMaturity_returns_404_for_unknown_skill()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/skills/{Guid.NewGuid()}/maturity");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
