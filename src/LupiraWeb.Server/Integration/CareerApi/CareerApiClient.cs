using System.Net;
using System.Net.Http.Json;
using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Integration.CareerApi;

/// <summary>
/// HTTP implementation of <see cref="ICareerApiClient"/>. Reads CareerApi's public, handle-addressed surface
/// (<c>/public/{handle}/…</c>) — the owner is selected by <c>CareerApi:PublicHandle</c>, the items are already
/// filtered to the published subset upstream, and the call is gated by a machine token attached by
/// <see cref="Auth.CareerApiAuthHandler"/>, so this class is unaware of how it is authenticated.
/// </summary>
internal sealed class CareerApiClient : ICareerApiClient
{
    private readonly HttpClient _http;
    private readonly string _prefix;

    public CareerApiClient(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        var handle = configuration["CareerApi:PublicHandle"]
            ?? throw new InvalidOperationException("CareerApi:PublicHandle is required");
        _prefix = $"/public/{handle}";
    }

    public Task<CareerProfileDto?> GetProfileAsync(CancellationToken ct) =>
        GetAsync<CareerProfileDto>($"{_prefix}/profile", ct);

    public Task<IReadOnlyList<CareerEngagementDto>> GetEngagementsAsync(CancellationToken ct) =>
        GetListAsync<CareerEngagementDto>($"{_prefix}/engagements", ct);

    public Task<CareerEngagementDto?> GetEngagementAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerEngagementDto>($"{_prefix}/engagements/{id}", ct);

    public Task<IReadOnlyList<CareerProjectDto>> GetProjectsAsync(Guid? engagementId, CancellationToken ct) =>
        // The public surface lists all published projects; it has no per-engagement filter (callers pass null).
        GetListAsync<CareerProjectDto>($"{_prefix}/projects", ct);

    public Task<CareerProjectDto?> GetProjectAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerProjectDto>($"{_prefix}/projects/{id}", ct);

    public Task<IReadOnlyList<CareerSkillDto>> GetSkillsAsync(CancellationToken ct) =>
        GetListAsync<CareerSkillDto>($"{_prefix}/skills", ct);

    public Task<CareerSkillDto?> GetSkillAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerSkillDto>($"{_prefix}/skills/{id}", ct);

    public Task<CareerSkillTimelineDto?> GetSkillTimelineAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerSkillTimelineDto>($"{_prefix}/skills/{id}/timeline", ct);

    public Task<CareerSkillMaturityDto?> GetSkillMaturityAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerSkillMaturityDto>($"{_prefix}/skills/{id}/maturity", ct);

    public Task<IReadOnlyList<CareerExperienceItemDto>> GetExperienceAsync(CancellationToken ct) =>
        GetListAsync<CareerExperienceItemDto>($"{_prefix}/experience", ct);

    public Task<IReadOnlyList<CareerMediaDto>> GetMediaAsync(CancellationToken ct) =>
        GetListAsync<CareerMediaDto>($"{_prefix}/media", ct);

    public Task<CareerMediaDto?> GetMediaAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerMediaDto>($"{_prefix}/media/{id}", ct);

    public Task<IReadOnlyList<CareerArtifactDto>> GetArtifactsAsync(CancellationToken ct) =>
        GetListAsync<CareerArtifactDto>($"{_prefix}/artifacts", ct);

    public Task<CareerArtifactDto?> GetArtifactAsync(Guid id, CancellationToken ct) =>
        GetAsync<CareerArtifactDto>($"{_prefix}/artifacts/{id}", ct);

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct) where T : class
    {
        using var response = await _http.GetAsync(path, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(CareerApiJson.Options, ct);
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string path, CancellationToken ct)
    {
        var list = await _http.GetFromJsonAsync<List<T>>(path, CareerApiJson.Options, ct);
        return list ?? [];
    }
}
