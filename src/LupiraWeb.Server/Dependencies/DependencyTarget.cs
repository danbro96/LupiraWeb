namespace LupiraWeb.Server.Dependencies;

/// <summary>One outward edge; auth mirrors the real client (Bearer or X-API-Key).</summary>
public sealed class DependencyTarget
{
    public required string Name { get; set; }
    public required string BaseUrl { get; set; }
    public required string ProbePath { get; set; }
    public string? Bearer { get; set; }
    public string? ApiKey { get; set; }
}

/// <summary>Roster derived from the same config keys the demo clients bind — edges cannot drift.
/// CareerApi is deliberately absent: it is a hard dependency and stays on /readyz.</summary>
public static class DependencyTargets
{
    public static IReadOnlyList<DependencyTarget> From(IConfiguration config) =>
    [
        new DependencyTarget
        {
            Name = "gpt-api",
            BaseUrl = config["Demos:Chat:BaseUrl"] ?? "",
            ProbePath = "v1/models",
            Bearer = NullIfBlank(config["Demos:Chat:ApiKey"]),
        },
        new DependencyTarget
        {
            Name = "kokoro-api",
            BaseUrl = config["Demos:TextToSpeech:BaseUrl"] ?? "",
            ProbePath = "options",
            ApiKey = NullIfBlank(config["Demos:TextToSpeech:ApiKey"]),
        },
        new DependencyTarget
        {
            Name = "florence-api",
            BaseUrl = config["Demos:Vision:BaseUrl"] ?? "",
            ProbePath = "options",
            ApiKey = NullIfBlank(config["Demos:Vision:ApiKey"]),
        },
    ];

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
