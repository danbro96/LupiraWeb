using LupiraWeb.Server.Data.Entities;

namespace LupiraWeb.Server.Data.Repositories;

public interface ISkillRepository
{
    Task<IReadOnlyList<SkillEntity>> ListAsync(CancellationToken ct);
}
