namespace LupiraWeb.Server.Endpoints.Demos.Vision.Dtos;

public sealed record Detection
{
    public required double X1 { get; set; }
    public required double Y1 { get; set; }
    public required double X2 { get; set; }
    public required double Y2 { get; set; }
    public required string Label { get; set; }
}
