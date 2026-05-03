using System.Net.Http.Json;
using System.Text.Json.Serialization;
using LupiraWeb.Server.Endpoints.Demos.Vision.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LupiraWeb.Server.Endpoints.Demos.Vision;

public sealed class VisionHandler(HttpClient client, ILogger<VisionHandler> logger)
{
    public async Task<Results<Ok<CaptionResponse>, ProblemHttpResult>> CaptionAsync(
        IFormFile image, CancellationToken ct)
    {
        var imageBase64 = await ReadAsBase64Async(image, ct);
        var upstreamResult = await PostUpstreamAsync<UpstreamCaptionResponse>(
            "/captions", new { image = imageBase64 }, ct);

        return upstreamResult.IsSuccess
            ? TypedResults.Ok(new CaptionResponse { Caption = upstreamResult.Value!.Caption ?? string.Empty })
            : upstreamResult.Problem!;
    }

    public async Task<Results<Ok<OcrResponse>, ProblemHttpResult>> OcrAsync(
        IFormFile image, CancellationToken ct)
    {
        var imageBase64 = await ReadAsBase64Async(image, ct);
        var upstreamResult = await PostUpstreamAsync<UpstreamOcrResponse>(
            "/ocr", new { image = imageBase64 }, ct);

        return upstreamResult.IsSuccess
            ? TypedResults.Ok(new OcrResponse { Text = upstreamResult.Value!.Text ?? string.Empty })
            : upstreamResult.Problem!;
    }

    public async Task<Results<Ok<DetectionResponse>, ProblemHttpResult>> DetectAsync(
        IFormFile image, CancellationToken ct)
    {
        var imageBase64 = await ReadAsBase64Async(image, ct);
        var upstreamResult = await PostUpstreamAsync<UpstreamDetectionResponse>(
            "/detections", new { image = imageBase64 }, ct);

        if (!upstreamResult.IsSuccess)
            return upstreamResult.Problem!;

        var bboxes = upstreamResult.Value!.Bboxes ?? [];
        var labels = upstreamResult.Value!.Labels ?? [];
        var items = bboxes.Zip(labels, (box, label) => new Detection
            {
                X1 = box.Length > 0 ? box[0] : 0d,
                Y1 = box.Length > 1 ? box[1] : 0d,
                X2 = box.Length > 2 ? box[2] : 0d,
                Y2 = box.Length > 3 ? box[3] : 0d,
                Label = label,
            })
            .ToList();

        return TypedResults.Ok(new DetectionResponse { Items = items });
    }

    private async Task<UpstreamResult<T>> PostUpstreamAsync<T>(
        string path, object body, CancellationToken ct) where T : class
    {
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync(path, body, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Vision upstream unreachable for {Path}", path);
            return UpstreamResult<T>.Fail(TypedResults.Problem(
                detail: "The vision service is unreachable.",
                statusCode: StatusCodes.Status502BadGateway));
        }

        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("Vision upstream {Path} returned {Status}: {Body}", path, response.StatusCode, raw);
            return UpstreamResult<T>.Fail(TypedResults.Problem(
                detail: $"Vision service returned {(int)response.StatusCode}.",
                statusCode: StatusCodes.Status502BadGateway));
        }

        var parsed = await response.Content.ReadFromJsonAsync<T>(ct)
            ?? throw new InvalidOperationException($"Empty body from vision upstream at {path}");
        return UpstreamResult<T>.Success(parsed);
    }

    private static async Task<string> ReadAsBase64Async(IFormFile image, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await image.CopyToAsync(ms, ct);
        return Convert.ToBase64String(ms.ToArray());
    }

    private sealed record UpstreamResult<T>(bool IsSuccess, T? Value, ProblemHttpResult? Problem)
        where T : class
    {
        public static UpstreamResult<T> Success(T value) => new(true, value, null);
        public static UpstreamResult<T> Fail(ProblemHttpResult problem) => new(false, default, problem);
    }

    private sealed record UpstreamCaptionResponse
    {
        [JsonPropertyName("caption")] public string? Caption { get; set; }
    }

    private sealed record UpstreamOcrResponse
    {
        [JsonPropertyName("text")] public string? Text { get; set; }
    }

    private sealed record UpstreamDetectionResponse
    {
        [JsonPropertyName("bboxes")] public List<double[]>? Bboxes { get; set; }
        [JsonPropertyName("labels")] public List<string>? Labels { get; set; }
    }
}
