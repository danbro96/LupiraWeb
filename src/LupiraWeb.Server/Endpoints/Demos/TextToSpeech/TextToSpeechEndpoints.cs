using LupiraWeb.Server.Endpoints.Demos.TextToSpeech.Dtos;

namespace LupiraWeb.Server.Endpoints.Demos.TextToSpeech;

public static class TextToSpeechEndpoints
{
    public static IEndpointRouteBuilder MapTextToSpeechEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/demos/text-to-speech").WithTags("Demos.TextToSpeech");

        group.MapGet("/voices",
                (TextToSpeechHandler handler, CancellationToken ct) => handler.GetVoicesAsync(ct))
            .WithName("DemoTextToSpeechGetVoices");

        group.MapPost("/syntheses",
                (SynthesizeRequest body, TextToSpeechHandler handler, CancellationToken ct)
                    => handler.SynthesizeAsync(body, ct))
            .WithName("DemoTextToSpeechSynthesize");

        return app;
    }
}
