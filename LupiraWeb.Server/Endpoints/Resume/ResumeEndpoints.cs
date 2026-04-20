namespace LupiraWeb.Server.Endpoints.Resume;

public static class ResumeEndpoints
{
    public static IEndpointRouteBuilder MapResumeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/resume").WithTags("Resume");

        group.MapGet("/me",
                (ResumeHandler handler, CancellationToken ct) => handler.GetMeAsync(ct))
            .WithName("GetMyInfo");

        group.MapGet("/employments",
                (ResumeHandler handler, CancellationToken ct) => handler.GetEmploymentsAsync(ct))
            .WithName("GetEmployments");

        group.MapGet("/employments/{id:guid}",
                (Guid id, ResumeHandler handler, CancellationToken ct) => handler.GetEmploymentAsync(id, ct))
            .WithName("GetEmployment");

        group.MapGet("/projects",
                (ResumeHandler handler, CancellationToken ct) => handler.GetProjectsAsync(ct))
            .WithName("GetProjects");

        group.MapGet("/projects/{id:guid}",
                (Guid id, ResumeHandler handler, CancellationToken ct) => handler.GetProjectAsync(id, ct))
            .WithName("GetProject");

        group.MapGet("/skills",
                (ResumeHandler handler, CancellationToken ct) => handler.GetSkillsAsync(ct))
            .WithName("GetSkills");

        return app;
    }
}
