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

        return app;
    }
}
