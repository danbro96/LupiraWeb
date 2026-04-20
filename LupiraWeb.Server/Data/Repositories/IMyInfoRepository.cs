using LupiraWeb.Server.Domain;

namespace LupiraWeb.Server.Data.Repositories;

public interface IMyInfoRepository
{
    Task<MyInfo?> GetAsync(CancellationToken ct);
}
