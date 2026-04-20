namespace LupiraWeb.Server.Endpoints.Artifacts;

public static class ArtifactsEndpoints
{
    public static IEndpointRouteBuilder MapArtifactsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/artifacts").WithTags("Artifacts");

        group.MapGet("/",
                (ArtifactsHandler handler, CancellationToken ct) => handler.ListAsync(ct))
            .WithName("ListArtifacts");

        group.MapGet("/{id:guid}",
                (Guid id, ArtifactsHandler handler, CancellationToken ct) => handler.GetAsync(id, ct))
            .WithName("GetArtifact");

        return app;
    }
}
