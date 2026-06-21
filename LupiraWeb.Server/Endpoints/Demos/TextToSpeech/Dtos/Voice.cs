namespace LupiraWeb.Server.Endpoints.Demos.TextToSpeech.Dtos;

public sealed class Voice
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Language { get; set; }
    public required string Gender { get; set; }
}
