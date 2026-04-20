using LupiraWeb.Server.Domain;
using LupiraWeb.Server.Endpoints.Experiences.Dtos;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Experiences;

public class ExperiencesHandler(IQuerySession session)
{
    public async Task<Ok<IReadOnlyList<ExperienceDto>>> ListAsync(
        DateOnly? from,
        DateOnly? to,
        Guid? skillId,
        Guid? engagementId,
        CancellationToken ct)
    {
        var query = session.Query<ExperienceRow>().AsQueryable();

        if (from is DateOnly fromDate)
            query = query.Where(r => r.OccurredOn >= fromDate);

        if (to is DateOnly toDate)
            query = query.Where(r => r.OccurredOn <= toDate);

        if (engagementId is Guid eid)
            query = query.Where(r => r.EngagementId == eid);

        var rows = await query.OrderByDescending(r => r.OccurredOn).ToListAsync(ct);

        // SkillId filter is applied in-memory because the SkillIds list doesn't translate cleanly
        // to Postgres JSON array membership queries without custom Marten plumbing.
        if (skillId is Guid sid)
            rows = rows.Where(r => r.SkillIds.Contains(sid)).ToList();

        return TypedResults.Ok<IReadOnlyList<ExperienceDto>>(
            rows.Select(ToDto).ToList());
    }

    private static ExperienceDto ToDto(ExperienceRow r) => new()
    {
        Id = r.Id,
        Kind = r.Kind,
        Title = r.Title,
        OccurredOn = r.OccurredOn,
        EndDate = r.EndDate,
        EngagementId = r.EngagementId,
        ProjectId = r.ProjectId,
        SkillIds = r.SkillIds.ToList(),
        Location = r.Location,
    };
}
