namespace LupiraWeb.Server.Endpoints.Demos.Vision.Dtos;

public sealed class DetectionResponse
{
    public required IReadOnlyList<Detection> Items { get; set; }
}
