namespace LupiraWeb.Server.Endpoints.Experiences;

public static class ExperiencesEndpoints
{
    public static IEndpointRouteBuilder MapExperiencesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/experiences").WithTags("Experiences");

        group.MapGet("/",
                (DateOnly? from, DateOnly? to, Guid? skillId, Guid? engagementId,
                 ExperiencesHandler handler, CancellationToken ct) =>
                    handler.ListAsync(from, to, skillId, engagementId, ct))
            .WithName("ListExperiences");

        return app;
    }
}
