using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using LupiraWeb.Server.Domain;
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
    public async Task Upload_then_get_roundtrips_metadata_and_blob()
    {
        var client = _factory.CreateClient();

        using var form = new MultipartFormDataContent();
        var bytes = Encoding.UTF8.GetBytes("fake-png-bytes");
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", "test.png");
        form.Add(new StringContent("Screenshot of dashboard"), "altText");
        form.Add(new StringContent("Caption here"), "caption");

        var uploadResponse = await client.PostAsync("/api/media", form);
        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);
        var upload = await uploadResponse.Content.ReadFromJsonAsync<MediaUploadResponse>();
        Assert.NotNull(upload);
        Assert.NotEqual(Guid.Empty, upload!.MediaId);
        Assert.StartsWith("memory://", upload.BlobRef);

        var getResponse = await client.GetAsync($"/api/media/{upload.MediaId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var asset = await getResponse.Content.ReadFromJsonAsync<MediaAssetDto>();
        Assert.NotNull(asset);
        Assert.Equal("image/png", asset!.MimeType);
        Assert.Equal("Screenshot of dashboard", asset.AltText);
        Assert.Equal("Caption here", asset.Caption);

        var blobResponse = await client.GetAsync($"/api/media/{upload.MediaId}/blob");
        Assert.Equal(HttpStatusCode.OK, blobResponse.StatusCode);
        var blobBytes = await blobResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(bytes, blobBytes);
    }

    [Fact]
    public async Task Get_returns_404_for_unknown_media()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/media/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Link_to_project_adds_to_linked_projects_list()
    {
        var client = _factory.CreateClient();

        var mediaId = await UploadTestImageAsync(client);

        var linkResponse = await client.PostAsJsonAsync(
            $"/api/media/{mediaId}/links/projects",
            new LinkMediaToProjectRequest
            {
                ProjectId = ResumeTestFactory.SeededProjectId,
                Role = MediaRole.Hero,
            });
        Assert.Equal(HttpStatusCode.NoContent, linkResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/media/{mediaId}");
        var asset = await getResponse.Content.ReadFromJsonAsync<MediaAssetDto>();
        Assert.NotNull(asset);
        Assert.Single(asset!.LinkedProjects);
        Assert.Equal(ResumeTestFactory.SeededProjectId, asset.LinkedProjects[0].ProjectId);
        Assert.Equal(MediaRole.Hero, asset.LinkedProjects[0].Role);
    }

    [Fact]
    public async Task Link_to_skills_adds_all_skill_ids()
    {
        var client = _factory.CreateClient();
        var mediaId = await UploadTestImageAsync(client);

        var linkResponse = await client.PostAsJsonAsync(
            $"/api/media/{mediaId}/links/skills",
            new LinkMediaToSkillsRequest
            {
                SkillIds = new[] { ResumeTestFactory.SeededSkillId },
                Note = "Showcase",
            });
        Assert.Equal(HttpStatusCode.NoContent, linkResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/media/{mediaId}");
        var asset = await getResponse.Content.ReadFromJsonAsync<MediaAssetDto>();
        Assert.NotNull(asset);
        Assert.Contains(ResumeTestFactory.SeededSkillId, asset!.LinkedSkillIds);
    }

    [Fact]
    public async Task List_includes_uploaded_media()
    {
        var client = _factory.CreateClient();
        var mediaId = await UploadTestImageAsync(client);

        var listResponse = await client.GetAsync("/api/media");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<MediaAssetDto>>();
        Assert.NotNull(list);
        Assert.Contains(list!, m => m.Id == mediaId);
    }

    private static async Task<Guid> UploadTestImageAsync(HttpClient client)
    {
        using var form = new MultipartFormDataContent();
        var bytes = Encoding.UTF8.GetBytes("fake-bytes");
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fileContent, "file", "img.png");
        form.Add(new StringContent("alt text"), "altText");

        var response = await client.PostAsync("/api/media", form);
        response.EnsureSuccessStatusCode();
        var upload = await response.Content.ReadFromJsonAsync<MediaUploadResponse>();
        return upload!.MediaId;
    }
}
