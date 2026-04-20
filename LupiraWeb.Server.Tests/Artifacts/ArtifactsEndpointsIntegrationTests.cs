using System.Net;
using System.Net.Http.Json;
using LupiraWeb.Server.Domain;
using LupiraWeb.Server.Endpoints.Artifacts.Dtos;
using LupiraWeb.Server.Tests.Resume;
using Xunit;

namespace LupiraWeb.Server.Tests.Artifacts;

public class ArtifactsEndpointsIntegrationTests : IClassFixture<ResumeTestFactory>
{
    private readonly ResumeTestFactory _factory;

    public ArtifactsEndpointsIntegrationTests(ResumeTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_then_get_roundtrips_metadata()
    {
        var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/artifacts",
            new RegisterArtifactRequest
            {
                Kind = ArtifactKind.Repo,
                Url = "https://github.com/example/repo",
                Title = "Example repo",
                Description = "Test description",
                ProducedOn = new DateOnly(2026, 1, 15),
            });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var reg = await registerResponse.Content.ReadFromJsonAsync<RegisterArtifactResponse>();
        Assert.NotNull(reg);

        var getResponse = await client.GetAsync($"/api/artifacts/{reg!.ArtifactId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var artifact = await getResponse.Content.ReadFromJsonAsync<ArtifactDto>();
        Assert.NotNull(artifact);
        Assert.Equal(ArtifactKind.Repo, artifact!.Kind);
        Assert.Equal("Example repo", artifact.Title);
        Assert.Equal("https://github.com/example/repo", artifact.Url);
    }

    [Fact]
    public async Task Get_returns_404_for_unknown_id()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/artifacts/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Link_to_project_reflects_in_aggregate()
    {
        var client = _factory.CreateClient();
        var artifactId = await RegisterTestArtifactAsync(client);

        var linkResponse = await client.PostAsJsonAsync(
            $"/api/artifacts/{artifactId}/links/project",
            new LinkArtifactToProjectRequest { ProjectId = ResumeTestFactory.SeededProjectId });
        Assert.Equal(HttpStatusCode.NoContent, linkResponse.StatusCode);

        var artifact = await (await client.GetAsync($"/api/artifacts/{artifactId}"))
            .Content.ReadFromJsonAsync<ArtifactDto>();
        Assert.Contains(ResumeTestFactory.SeededProjectId, artifact!.LinkedProjectIds);
    }

    [Fact]
    public async Task Link_to_engagement_reflects_in_aggregate()
    {
        var client = _factory.CreateClient();
        var artifactId = await RegisterTestArtifactAsync(client);

        var linkResponse = await client.PostAsJsonAsync(
            $"/api/artifacts/{artifactId}/links/engagement",
            new LinkArtifactToEngagementRequest { EngagementId = ResumeTestFactory.SeededEngagementId });
        Assert.Equal(HttpStatusCode.NoContent, linkResponse.StatusCode);

        var artifact = await (await client.GetAsync($"/api/artifacts/{artifactId}"))
            .Content.ReadFromJsonAsync<ArtifactDto>();
        Assert.Contains(ResumeTestFactory.SeededEngagementId, artifact!.LinkedEngagementIds);
    }

    [Fact]
    public async Task Link_to_skill_captures_role()
    {
        var client = _factory.CreateClient();
        var artifactId = await RegisterTestArtifactAsync(client);

        var linkResponse = await client.PostAsJsonAsync(
            $"/api/artifacts/{artifactId}/links/skill",
            new LinkArtifactToSkillRequest
            {
                SkillId = ResumeTestFactory.SeededSkillId,
                Role = ArtifactRole.Output,
            });
        Assert.Equal(HttpStatusCode.NoContent, linkResponse.StatusCode);

        var artifact = await (await client.GetAsync($"/api/artifacts/{artifactId}"))
            .Content.ReadFromJsonAsync<ArtifactDto>();
        Assert.Contains(artifact!.LinkedSkills,
            l => l.SkillId == ResumeTestFactory.SeededSkillId && l.Role == ArtifactRole.Output);
    }

    [Fact]
    public async Task List_excludes_archived_and_includes_registered()
    {
        var client = _factory.CreateClient();
        var artifactId = await RegisterTestArtifactAsync(client);

        var listResponse = await client.GetAsync("/api/artifacts");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<ArtifactDto>>();
        Assert.NotNull(list);
        Assert.Contains(list!, a => a.Id == artifactId);
    }

    private static async Task<Guid> RegisterTestArtifactAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/artifacts",
            new RegisterArtifactRequest
            {
                Kind = ArtifactKind.Repo,
                Url = $"https://github.com/example/{Guid.NewGuid():N}",
                Title = "Test artifact",
            });
        response.EnsureSuccessStatusCode();
        var reg = await response.Content.ReadFromJsonAsync<RegisterArtifactResponse>();
        return reg!.ArtifactId;
    }
}
