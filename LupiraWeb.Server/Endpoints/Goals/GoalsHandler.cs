using LupiraWeb.Server.Domain;
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

    public async Task<Ok<SetGoalResponse>> SetAsync(SetGoalRequest request, CancellationToken ct)
    {
        var goalId = Guid.CreateVersion7();
        session.Events.StartStream<Goal>(goalId,
            new GoalSet(
                goalId,
                request.SkillId,
                request.TargetMaturity,
                request.Deadline,
                request.Motivation,
                DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return TypedResults.Ok(new SetGoalResponse { GoalId = goalId });
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> RecordProgressAsync(
        Guid id, RecordProgressRequest request, CancellationToken ct)
    {
        var goal = await session.LoadAsync<Goal>(id, ct);
        if (goal is null) return TypedResults.NotFound();
        if (goal.Status != GoalStatus.Active)
            return TypedResults.BadRequest("Goal is already resolved");

        session.Events.Append(id,
            new GoalProgressRecorded(id, request.Note, request.LinkedEventId, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> AchieveAsync(
        Guid id, AchieveGoalRequest request, CancellationToken ct)
    {
        var goal = await session.LoadAsync<Goal>(id, ct);
        if (goal is null) return TypedResults.NotFound();
        if (goal.Status != GoalStatus.Active)
            return TypedResults.BadRequest("Goal is already resolved");

        session.Events.Append(id,
            new GoalAchieved(id, request.AchievedOn, request.EvidenceArtifactId, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound, BadRequest<string>>> AbandonAsync(
        Guid id, AbandonGoalRequest request, CancellationToken ct)
    {
        var goal = await session.LoadAsync<Goal>(id, ct);
        if (goal is null) return TypedResults.NotFound();
        if (goal.Status != GoalStatus.Active)
            return TypedResults.BadRequest("Goal is already resolved");

        session.Events.Append(id,
            new GoalAbandoned(id, request.AbandonedOn, request.Reason, DateTimeOffset.UtcNow));
        await session.SaveChangesAsync(ct);
        return TypedResults.NoContent();
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
