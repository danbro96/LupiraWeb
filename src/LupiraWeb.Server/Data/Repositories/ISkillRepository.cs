using LupiraWeb.Server.Integration.CareerApi.Dtos;

namespace LupiraWeb.Server.Data.Repositories;

public interface ISkillRepository
{
    Task<IReadOnlyList<CareerSkillDto>> ListAsync(CancellationToken ct);
}
