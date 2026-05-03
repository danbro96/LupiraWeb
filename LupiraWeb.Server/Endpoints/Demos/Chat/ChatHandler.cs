using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LupiraWeb.Server.Endpoints.Demos.Chat.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Demos.Chat;

public sealed class ChatHandler(HttpClient client, ILogger<ChatHandler> logger)
{
    private const string Model = "qwen3-8b";

    public async Task<Results<Ok<ChatResponse>, ProblemHttpResult>> SendAsync(
        ChatRequest req, CancellationToken ct)
    {
        var upstream = new UpstreamRequest
        {
            Model = Model,
            Messages = [new UpstreamMessage { Role = "user", Content = req.Prompt }],
        };

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("/v1/chat/completions", upstream, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Chat upstream unreachable");
            return TypedResults.Problem(
                detail: "The chat service is unreachable.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Chat upstream returned {Status}: {Body}", response.StatusCode, body);
            return TypedResults.Problem(
                detail: $"Chat service returned {(int)response.StatusCode}.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        var parsed = await response.Content.ReadFromJsonAsync<UpstreamResponse>(ct);
        var reply = parsed?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

        return TypedResults.Ok(new ChatResponse { Reply = reply });
    }

    private sealed record UpstreamRequest
    {
        [JsonPropertyName("model")] public required string Model { get; set; }
        [JsonPropertyName("messages")] public required IReadOnlyList<UpstreamMessage> Messages { get; set; }
    }

    private sealed record UpstreamMessage
    {
        [JsonPropertyName("role")] public required string Role { get; set; }
        [JsonPropertyName("content")] public required string Content { get; set; }
    }

    private sealed record UpstreamResponse
    {
        [JsonPropertyName("choices")] public List<UpstreamChoice>? Choices { get; set; }
    }

    private sealed record UpstreamChoice
    {
        [JsonPropertyName("message")] public UpstreamMessage? Message { get; set; }
    }
}
