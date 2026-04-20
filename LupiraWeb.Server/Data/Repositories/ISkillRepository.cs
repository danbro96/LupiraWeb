using LupiraWeb.Domain;

namespace LupiraWeb.Server.Data.Repositories;

public interface ISkillRepository
{
    Task<IReadOnlyList<Skill>> ListAsync(CancellationToken ct);
}
