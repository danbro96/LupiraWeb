namespace LupiraWeb.Server.Endpoints.Demos.Vision.Dtos;

public sealed record OcrResponse
{
    public required string Text { get; set; }
}
