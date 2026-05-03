using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LupiraWeb.Server.Endpoints.Demos.TextToSpeech.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Demos.TextToSpeech;

public sealed class TextToSpeechHandler(HttpClient client, ILogger<TextToSpeechHandler> logger)
{
    private static readonly JsonSerializerOptions OmitNulls = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<Results<Ok<IReadOnlyList<Voice>>, ProblemHttpResult>> GetVoicesAsync(
        CancellationToken ct)
    {
        UpstreamOptions? options;
        try
        {
            options = await client.GetFromJsonAsync<UpstreamOptions>("/options", ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Text-to-speech upstream unreachable");
            return TypedResults.Problem(
                detail: "The text-to-speech service is unreachable.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        var voices = (options?.Voices ?? [])
            .Select(v => new Voice
            {
                Id = v.Id,
                Name = v.Name,
                Language = v.Language,
                Gender = v.Gender,
            })
            .ToList();

        return TypedResults.Ok<IReadOnlyList<Voice>>(voices);
    }

    public async Task<Results<FileStreamHttpResult, ProblemHttpResult>> SynthesizeAsync(
        SynthesizeRequest req, CancellationToken ct)
    {
        var upstream = new UpstreamSynthesizeRequest
        {
            Text = req.Text,
            Voice = req.Voice,
            Speed = req.Speed,
        };

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("/tts", upstream, OmitNulls, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Text-to-speech upstream unreachable");
            return TypedResults.Problem(
                detail: "The text-to-speech service is unreachable.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Text-to-speech upstream returned {Status}: {Body}", response.StatusCode, body);
            return TypedResults.Problem(
                detail: $"Text-to-speech service returned {(int)response.StatusCode}.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        var stream = await response.Content.ReadAsStreamAsync(ct);
        return TypedResults.File(stream, "audio/wav");
    }

    private sealed record UpstreamOptions
    {
        [JsonPropertyName("voices")] public List<UpstreamVoice>? Voices { get; set; }
    }

    private sealed record UpstreamVoice
    {
        [JsonPropertyName("id")] public required string Id { get; set; }
        [JsonPropertyName("name")] public required string Name { get; set; }
        [JsonPropertyName("language")] public required string Language { get; set; }
        [JsonPropertyName("gender")] public required string Gender { get; set; }
    }

    private sealed record UpstreamSynthesizeRequest
    {
        [JsonPropertyName("text")] public required string Text { get; set; }
        [JsonPropertyName("voice")] public string? Voice { get; set; }
        [JsonPropertyName("speed")] public double? Speed { get; set; }
    }
}
