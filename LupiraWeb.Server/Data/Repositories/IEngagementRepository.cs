using LupiraWeb.Domain;

namespace LupiraWeb.Server.Data.Repositories;

public interface IEngagementRepository
{
    Task<IReadOnlyList<Engagement>> ListAsync(CancellationToken ct);
    Task<Engagement?> GetAsync(Guid id, CancellationToken ct);
}
