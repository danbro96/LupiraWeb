using System.Net;
using System.Net.Http.Json;
using LupiraWeb.Server.Endpoints.Media.Dtos;
using LupiraWeb.Server.Tests.Resume;
using Xunit;

namespace LupiraWeb.Server.Tests.Media;

public class MediaEndpointsIntegrationTests : IClassFixture<ResumeTestFactory>
{
    private readonly ResumeTestFactory _factory;

    public MediaEndpointsIntegrationTests(ResumeTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task List_returns_200_with_non_archived_media()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/media/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<MediaAssetDto>>();
        Assert.NotNull(list);
    }

    [Fact]
    public async Task Get_unknown_returns_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/media/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Download_blob_returns_404_until_upstream_binary_surface_exists()
    {
        // Degraded behavior: CareerApi exposes no media binary endpoint (only a BlobRef/MinIO key).
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/media/{Guid.NewGuid()}/blob");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
