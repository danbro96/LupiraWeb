using LupiraWeb.Server.Endpoints.Goals.Dtos;

namespace LupiraWeb.Server.Endpoints.Goals;

public static class GoalsEndpoints
{
    public static IEndpointRouteBuilder MapGoalsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/goals").WithTags("Goals");

        group.MapGet("/",
                (GoalsHandler handler, CancellationToken ct) => handler.ListAsync(ct))
            .WithName("ListGoals");

        group.MapGet("/{id:guid}",
                (Guid id, GoalsHandler handler, CancellationToken ct) => handler.GetAsync(id, ct))
            .WithName("GetGoal");

        group.MapPost("/",
                (SetGoalRequest request, GoalsHandler handler, CancellationToken ct) =>
                    handler.SetAsync(request, ct))
            .WithName("SetGoal");

        group.MapPost("/{id:guid}/progress",
                (Guid id, RecordProgressRequest request,
                 GoalsHandler handler, CancellationToken ct) =>
                    handler.RecordProgressAsync(id, request, ct))
            .WithName("RecordGoalProgress");

        group.MapPost("/{id:guid}/achieve",
                (Guid id, AchieveGoalRequest request,
                 GoalsHandler handler, CancellationToken ct) =>
                    handler.AchieveAsync(id, request, ct))
            .WithName("AchieveGoal");

        group.MapPost("/{id:guid}/abandon",
                (Guid id, AbandonGoalRequest request,
                 GoalsHandler handler, CancellationToken ct) =>
                    handler.AbandonAsync(id, request, ct))
            .WithName("AbandonGoal");

        return app;
    }
}
