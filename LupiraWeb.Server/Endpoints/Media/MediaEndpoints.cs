namespace LupiraWeb.Server.Endpoints.Media;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media").WithTags("Media");

        group.MapGet("/",
                (MediaHandler handler, CancellationToken ct) => handler.ListAsync(ct))
            .WithName("ListMedia");

        group.MapGet("/{id:guid}",
                (Guid id, MediaHandler handler, CancellationToken ct) => handler.GetAsync(id, ct))
            .WithName("GetMedia");

        group.MapGet("/{id:guid}/blob",
                (Guid id, MediaHandler handler, CancellationToken ct) => handler.DownloadAsync(id, ct))
            .WithName("DownloadMedia");

        return app;
    }
}
