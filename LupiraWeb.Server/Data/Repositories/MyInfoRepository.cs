using LupiraWeb.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LupiraWeb.Server.Data.Repositories;

internal sealed class MyInfoRepository(AppDbContext db) : IMyInfoRepository
{
    public Task<MyInfoEntity?> GetAsync(CancellationToken ct) =>
        db.MyInfo.AsNoTracking().FirstOrDefaultAsync(ct);
}
