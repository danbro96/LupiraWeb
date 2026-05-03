namespace LupiraWeb.Server.Endpoints.Demos.Vision.Dtos;

public sealed record DetectionResponse
{
    public required IReadOnlyList<Detection> Items { get; set; }
}
