using LupiraWeb.Domain;
using LupiraWeb.Server.Endpoints.Goals.Dtos;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Goals;

public class GoalsHandler(IDocumentSession session)
{
    public async Task<Ok<IReadOnlyList<GoalDto>>> ListAsync(CancellationToken ct)
    {
        var goals = await session.Query<Goal>().ToListAsync(ct);
        return TypedResults.Ok<IReadOnlyList<GoalDto>>(goals.Select(ToDto).ToList());
    }

    public async Task<Results<Ok<GoalDto>, NotFound>> GetAsync(Guid id, CancellationToken ct)
    {
        var goal = await session.LoadAsync<Goal>(id, ct);
        if (goal is null) return TypedResults.NotFound();
        return TypedResults.Ok(ToDto(goal));
    }

    private static GoalDto ToDto(Goal g) => new()
    {
        Id = g.Id,
        SkillId = g.SkillId,
        TargetMaturity = g.TargetMaturity,
        Deadline = g.Deadline,
        Motivation = g.Motivation,
        Status = g.Status,
        ResolvedAt = g.ResolvedAt,
        ResolutionReason = g.ResolutionReason,
        EvidenceArtifactId = g.EvidenceArtifactId,
        Progress = g.Progress.Select(p => new GoalProgressEntryDto
        {
            RecordedAt = p.RecordedAt,
            Note = p.Note,
            LinkedEventId = p.LinkedEventId,
        }).ToList(),
    };
}
