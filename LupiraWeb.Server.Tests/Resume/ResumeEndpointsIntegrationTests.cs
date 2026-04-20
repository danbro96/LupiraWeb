using System.Net;
using System.Net.Http.Json;
using LupiraWeb.Server.Endpoints.Resume.Dtos;
using Xunit;

namespace LupiraWeb.Server.Tests.Resume;

public class ResumeEndpointsIntegrationTests : IClassFixture<ResumeTestFactory>
{
    private readonly ResumeTestFactory _factory;

    public ResumeEndpointsIntegrationTests(ResumeTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMe_returns_200_with_seeded_user()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/resume/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await response.Content.ReadFromJsonAsync<MyInfo>();
        Assert.NotNull(me);
        Assert.Equal("Test User", me!.FullName);
    }

    [Fact]
    public async Task GetEngagements_returns_200_with_list()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/resume/engagements");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<Engagement>>();
        Assert.NotNull(list);
        Assert.Contains(list!, e => e.Institution == "Strivo");
    }

    [Fact]
    public async Task GetEngagement_by_id_returns_200_for_seeded_id()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/resume/engagements/{ResumeTestFactory.SeededEngagementId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var eng = await response.Content.ReadFromJsonAsync<Engagement>();
        Assert.NotNull(eng);
        Assert.Equal("Strivo", eng!.Institution);
        Assert.Equal("Consultant", eng.Title);
        Assert.Contains(eng.Skills, s => s.Name == "C#");
        Assert.Contains(eng.Projects, p => p.Title == "LupiraWeb");
    }

    [Fact]
    public async Task GetEngagement_by_id_returns_404_for_unknown_id()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/resume/engagements/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_returns_200_with_list()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/resume/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<Project>>();
        Assert.NotNull(list);
        Assert.Contains(list!, p => p.Title == "LupiraWeb");
    }

    [Fact]
    public async Task GetProject_by_id_returns_404_for_unknown_id()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/resume/projects/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSkills_returns_200_with_list()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/resume/skills");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<Skill>>();
        Assert.NotNull(list);
        Assert.Contains(list!, s => s.Name == "C#");
    }

    [Fact]
    public async Task Health_liveness_returns_200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_readiness_returns_200_when_db_reachable()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Openapi_document_lists_resume_routes()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/resume/me", json);
        Assert.Contains("/api/resume/engagements", json);
        Assert.Contains("/api/resume/skills", json);
        Assert.Contains("/api/resume/projects", json);
    }
}
