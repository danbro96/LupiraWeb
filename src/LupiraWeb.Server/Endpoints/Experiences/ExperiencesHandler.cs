using LupiraWeb.Server.Contracts;
using LupiraWeb.Server.Endpoints.Experiences.Dtos;
using LupiraWeb.Server.Integration.CareerApi;
using LupiraWeb.Server.Integration.CareerApi.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Experiences;

public class ExperiencesHandler(ICareerApiClient client)
{
    public async Task<Ok<IReadOnlyList<ExperienceDto>>> ListAsync(
        DateOnly? from,
        DateOnly? to,
        Guid? skillId,
        Guid? engagementId,
        CancellationToken ct)
    {
        // TODO(escalation): CareerApi's /api/experience has no server-side filters, so we fetch the full
        // timeline and filter in-memory. Its rows also omit a project's parent engagement, so engagementId
        // can only match engagement rows (project rows carry no EngagementId upstream).
        var rows = (IEnumerable<CareerExperienceItemDto>)await client.GetExperienceAsync(ct);

        if (from is DateOnly fromDate)
            rows = rows.Where(r => r.OccurredOn >= fromDate);

        if (to is DateOnly toDate)
            rows = rows.Where(r => r.OccurredOn <= toDate);

        if (skillId is Guid sid)
            rows = rows.Where(r => r.SkillIds.Contains(sid));

        if (engagementId is Guid eid)
            rows = rows.Where(r => r.Kind == ExperienceKind.Engagement && r.Id == eid);

        var result = rows
            .OrderByDescending(r => r.OccurredOn)
            .Select(ToDto)
            .ToList();

        return TypedResults.Ok<IReadOnlyList<ExperienceDto>>(result);
    }

    private static ExperienceDto ToDto(CareerExperienceItemDto r) => new()
    {
        Id = r.Id,
        Kind = r.Kind,
        Title = r.Title,
        OccurredOn = r.OccurredOn,
        EndDate = r.EndDate,
        EngagementId = r.Kind == ExperienceKind.Engagement ? r.Id : null,
        ProjectId = r.Kind == ExperienceKind.Project ? r.Id : null,
        SkillIds = r.SkillIds.ToList(),
        Location = r.Location,
    };
}
