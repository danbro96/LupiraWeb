namespace LupiraWeb.Server.Endpoints.Demos.Chat.Dtos;

public sealed record ChatResponse
{
    public required string Reply { get; set; }
}
