using LupiraWeb.Admin.Server.Endpoints.Goals.Dtos;
using LupiraWeb.Domain;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Admin.Server.Endpoints.Goals;

public class GoalsHandler(IDocumentSession session)
{
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
}
