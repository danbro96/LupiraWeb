using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Integration.CareerApi;

/// <summary>
/// Typed read client over LupiraCareerApi. Single-item gets return <c>null</c> when CareerApi answers
/// 404, so callers can preserve their own NotFound branches.
/// </summary>
public interface ICareerApiClient
{
    Task<CareerMeDto?> GetMeAsync(CancellationToken ct);
    Task<CareerProfileDto?> GetProfileAsync(CancellationToken ct);
    Task<CareerResumeDto?> GetResumeAsync(CancellationToken ct);

    Task<IReadOnlyList<CareerEngagementDto>> GetEngagementsAsync(CancellationToken ct);
    Task<CareerEngagementDto?> GetEngagementAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<CareerProjectDto>> GetProjectsAsync(Guid? engagementId, CancellationToken ct);
    Task<CareerProjectDto?> GetProjectAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<CareerSkillDto>> GetSkillsAsync(CancellationToken ct);
    Task<CareerSkillDto?> GetSkillAsync(Guid id, CancellationToken ct);
    Task<CareerSkillTimelineDto?> GetSkillTimelineAsync(Guid id, CancellationToken ct);
    Task<CareerSkillMaturityDto?> GetSkillMaturityAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<CareerExperienceItemDto>> GetExperienceAsync(CancellationToken ct);

    Task<IReadOnlyList<CareerMediaDto>> GetMediaAsync(CancellationToken ct);
    Task<CareerMediaDto?> GetMediaAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<CareerArtifactDto>> GetArtifactsAsync(CancellationToken ct);
    Task<CareerArtifactDto?> GetArtifactAsync(Guid id, CancellationToken ct);
}
