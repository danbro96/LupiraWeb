using LupiraWeb.Server.Endpoints.Skills.Dtos;
using LupiraWeb.Server.Integration.CareerApi;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Skills;

public class SkillsHandler(ICareerApiClient client)
{
    public async Task<Results<Ok<SkillTimelineResponse>, NotFound>> GetTimelineAsync(
        Guid id, CancellationToken ct)
    {
        var timeline = await client.GetSkillTimelineAsync(id, ct);
        if (timeline is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(new SkillTimelineResponse
        {
            SkillId = timeline.Id,
            Name = timeline.Name,
            Entries = timeline.Entries.Select(e => new SkillTimelineEntryDto
            {
                Kind = e.Kind,
                OccurredOn = e.OccurredOn,
                ContextKind = e.ContextKind,
                ContextId = e.ContextId,
                ContextLabel = e.ContextLabel,
                Intensity = e.Intensity,
                Maturity = e.Maturity,
                OtherSkillId = e.OtherSkillId,
                Note = e.Note,
            }).ToList(),
        });
    }

    public async Task<Results<Ok<SkillRelatedResponse>, NotFound>> GetRelatedAsync(
        Guid id, CancellationToken ct)
    {
        // TODO(escalation): CareerApi exposes no skill adjacency/co-occurrence endpoint. We preserve the
        // route, response shape, and 404-on-unknown-skill contract, returning an empty Related list until
        // an upstream adjacency surface exists.
        var skill = await client.GetSkillAsync(id, ct);
        if (skill is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(new SkillRelatedResponse
        {
            SkillId = id,
            Related = [],
        });
    }

    public async Task<Results<Ok<SkillMaturityResponse>, NotFound>> GetMaturityAsync(
        Guid id, CancellationToken ct)
    {
        var maturity = await client.GetSkillMaturityAsync(id, ct);
        if (maturity is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(new SkillMaturityResponse
        {
            SkillId = maturity.Id,
            Current = maturity.Current,
            Trajectory = maturity.Trajectory.Select(p => new SkillMaturityPointDto
            {
                OccurredOn = p.OccurredOn,
                Maturity = p.Maturity,
                Reason = p.Reason,
            }).ToList(),
        });
    }
}
