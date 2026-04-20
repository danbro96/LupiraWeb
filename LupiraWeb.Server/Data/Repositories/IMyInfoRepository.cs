using LupiraWeb.Server.Data.Entities;

namespace LupiraWeb.Server.Data.Repositories;

public interface IMyInfoRepository
{
    Task<MyInfoEntity?> GetAsync(CancellationToken ct);
}
