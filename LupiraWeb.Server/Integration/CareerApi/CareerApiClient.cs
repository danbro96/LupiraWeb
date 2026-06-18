using System.Net;
using System.Net.Http.Json;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Integration.CareerApi;

/// <summary>
/// HTTP implementation of <see cref="ICareerApiClient"/>. The auth header (dev <c>X-Dev-User</c> or prod
/// bearer) is attached by <see cref="Auth.CareerApiAuthHandler"/> in the client's message pipeline, so
/// this class is unaware of how the call is authenticated.
/// </summary>
internal sealed class CareerApiClient(HttpClient http) : ICareerApiClient
{
    public Task<CareerMeDto?> GetMeAsync(CancellationToken ct) =>
        GetAsync<CareerMeDto>("/api/me", ct);

    public Task<CareerProfileDto?> GetProfileAsync(CancellationToken ct) =>
        GetAsync<CareerProfileDto>("/api/profile", ct);

    public Task<CareerResumeDto?> GetResumeAsync(CancellationToken ct) =>
        GetAsync<CareerResumeDto>("/api/resume", ct);

    public Task<IReadOnlyList<CareerEngagementDto>> GetEngagementsAsync(CancellationToken ct) =>
        GetListAsync<CareerEngagementDto>("/api/engagements", ct);

    public Task<CareerEngagementDto?> GetEngagementAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerEngagementDto>($"/api/engagements/{id}", ct);

    public Task<IReadOnlyList<CareerProjectDto>> GetProjectsAsync(Guid? engagementId, CancellationToken ct) =>
        GetListAsync<CareerProjectDto>(
            engagementId is Guid eid ? $"/api/projects?engagementId={eid}" : "/api/projects", ct);

    public Task<CareerProjectDto?> GetProjectAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerProjectDto>($"/api/projects/{id}", ct);

    public Task<IReadOnlyList<CareerSkillDto>> GetSkillsAsync(CancellationToken ct) =>
        GetListAsync<CareerSkillDto>("/api/skills", ct);

    public Task<CareerSkillDto?> GetSkillAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerSkillDto>($"/api/skills/{id}", ct);

    public Task<CareerSkillTimelineDto?> GetSkillTimelineAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerSkillTimelineDto>($"/api/skills/{id}/timeline", ct);

    public Task<CareerSkillMaturityDto?> GetSkillMaturityAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerSkillMaturityDto>($"/api/skills/{id}/maturity", ct);

    public Task<IReadOnlyList<CareerExperienceItemDto>> GetExperienceAsync(CancellationToken ct) =>
        GetListAsync<CareerExperienceItemDto>("/api/experience", ct);

    public Task<IReadOnlyList<CareerMediaDto>> GetMediaAsync(CancellationToken ct) =>
        GetListAsync<CareerMediaDto>("/api/media", ct);

    public Task<CareerMediaDto?> GetMediaAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerMediaDto>($"/api/media/{id}", ct);

    public Task<IReadOnlyList<CareerArtifactDto>> GetArtifactsAsync(CancellationToken ct) =>
        GetListAsync<CareerArtifactDto>("/api/artifacts", ct);

    public Task<CareerArtifactDto?> GetArtifactAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerArtifactDto>($"/api/artifacts/{id}", ct);

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct) where T : class
    {
        using var response = await http.GetAsync(path, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(CareerApiJson.Options, ct);
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string path, CancellationToken ct)
    {
        var list = await http.GetFromJsonAsync<List<T>>(path, CareerApiJson.Options, ct);
        return list ?? [];
    }
}
