using LupiraWeb.Domain;
using LupiraWeb.Server.Endpoints.Skills.Dtos;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
using SkillDocument = LupiraWeb.Domain.Skill;

namespace LupiraWeb.Server.Endpoints.Skills;

public class SkillsHandler(IQuerySession session)
{
    public async Task<Results<Ok<SkillTimelineResponse>, NotFound>> GetTimelineAsync(
        Guid id, CancellationToken ct)
    {
        var timeline = await session.LoadAsync<SkillTimeline>(id, ct);
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
        var skill = await session.LoadAsync<SkillDocument>(id, ct);
        if (skill is null)
            return TypedResults.NotFound();

        var rows = await session.Query<SkillAdjacencyRow>()
            .Where(r => r.SkillA == id || r.SkillB == id)
            .ToListAsync(ct);

        var otherIds = rows
            .Select(r => r.SkillA == id ? r.SkillB : r.SkillA)
            .Distinct()
            .ToList();

        var others = otherIds.Count == 0
            ? new List<SkillDocument>()
            : (await session.LoadManyAsync<SkillDocument>(ct, otherIds)).ToList();
        var namesById = others.ToDictionary(s => s.Id, s => s.Name);

        var related = rows
            .Select(r =>
            {
                var otherId = r.SkillA == id ? r.SkillB : r.SkillA;
                return new SkillRelatedEntry
                {
                    SkillId = otherId,
                    Name = namesById.TryGetValue(otherId, out var n) ? n : "",
                    Count = r.Count,
                    FirstSeen = r.FirstSeen,
                    LastSeen = r.LastSeen,
                };
            })
            .OrderByDescending(e => e.Count)
            .ThenBy(e => e.Name)
            .ToList();

        return TypedResults.Ok(new SkillRelatedResponse
        {
            SkillId = id,
            Related = related,
        });
    }

    public async Task<Results<Ok<SkillMaturityResponse>, NotFound>> GetMaturityAsync(
        Guid id, CancellationToken ct)
    {
        var maturity = await session.LoadAsync<SkillMaturity>(id, ct);
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
