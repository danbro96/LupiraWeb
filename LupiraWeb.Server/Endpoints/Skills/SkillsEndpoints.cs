namespace LupiraWeb.Server.Endpoints.Skills;

public static class SkillsEndpoints
{
    public static IEndpointRouteBuilder MapSkillsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/skills").WithTags("Skills");

        group.MapGet("/{id:guid}/timeline",
                (Guid id, SkillsHandler handler, CancellationToken ct) => handler.GetTimelineAsync(id, ct))
            .WithName("GetSkillTimeline");

        group.MapGet("/{id:guid}/related",
                (Guid id, SkillsHandler handler, CancellationToken ct) => handler.GetRelatedAsync(id, ct))
            .WithName("GetSkillRelated");

        group.MapGet("/{id:guid}/maturity",
                (Guid id, SkillsHandler handler, CancellationToken ct) => handler.GetMaturityAsync(id, ct))
            .WithName("GetSkillMaturity");

        return app;
    }
}
