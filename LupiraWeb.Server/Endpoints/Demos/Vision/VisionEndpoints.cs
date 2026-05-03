namespace LupiraWeb.Server.Endpoints.Demos.Vision;

public static class VisionEndpoints
{
    public static IEndpointRouteBuilder MapVisionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/demos/vision")
            .WithTags("Demos.Vision")
            .DisableAntiforgery();

        group.MapPost("/captions",
                (IFormFile image, VisionHandler handler, CancellationToken ct)
                    => handler.CaptionAsync(image, ct))
            .WithName("DemoVisionCaption");

        group.MapPost("/ocr",
                (IFormFile image, VisionHandler handler, CancellationToken ct)
                    => handler.OcrAsync(image, ct))
            .WithName("DemoVisionOcr");

        group.MapPost("/detections",
                (IFormFile image, VisionHandler handler, CancellationToken ct)
                    => handler.DetectAsync(image, ct))
            .WithName("DemoVisionDetect");

        return app;
    }
}
