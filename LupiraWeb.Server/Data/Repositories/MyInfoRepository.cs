using LupiraWeb.Server.Domain;
using Marten;

namespace LupiraWeb.Server.Data.Repositories;

internal sealed class MyInfoRepository(IQuerySession session) : IMyInfoRepository
{
    public Task<MyInfo?> GetAsync(CancellationToken ct) =>
        session.LoadAsync<MyInfo>(MyInfo.SingletonId, ct);
}
