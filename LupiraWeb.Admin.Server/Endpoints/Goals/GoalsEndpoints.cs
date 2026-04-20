using LupiraWeb.Admin.Server.Endpoints.Goals.Dtos;

namespace LupiraWeb.Admin.Server.Endpoints.Goals;

public static class GoalsEndpoints
{
    public static IEndpointRouteBuilder MapGoalsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/goals").WithTags("Goals");

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
