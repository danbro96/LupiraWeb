namespace LupiraWeb.Server.Endpoints.Demos.Chat.Dtos;

public sealed record ChatRequest
{
    public required string Prompt { get; set; }
}
