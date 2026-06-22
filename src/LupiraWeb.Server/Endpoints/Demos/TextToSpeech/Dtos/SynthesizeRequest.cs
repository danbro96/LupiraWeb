namespace LupiraWeb.Server.Endpoints.Demos.TextToSpeech.Dtos;

public sealed class SynthesizeRequest
{
    public required string Text { get; set; }
    public string? Voice { get; set; }
    public double? Speed { get; set; }
}
