using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var api = app.MapGroup("/api");

api.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapFallbackToFile("/index.html");

// Start the app, then fetch the generated OpenAPI JSON ("/openapi/v1.json") and write it into the frontend project
// so Orval can generate the client artifacts from a local copy on each dev start.
await app.StartAsync();

if (app.Environment.IsDevelopment())
{
    try
    {
        // Try to discover a listening address
        var addressesFeature = app.Services.GetRequiredService<IServer>();
        var serverAddressesFeature = addressesFeature.Features.Get<IServerAddressesFeature>();
        var address = serverAddressesFeature?.Addresses.FirstOrDefault() ?? app.Urls.FirstOrDefault() ?? "http://localhost:5000";
        if (string.IsNullOrEmpty(address))
        {
            Console.WriteLine("Skipping OpenAPI fetch: no concrete server address available.");
        }
        else
        {
            if (!address.EndsWith("/")) address += "/";
            var openApiUri = new Uri(new Uri(address), "openapi/v1.json");

            using var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };

            var openApiContent = await client.GetStringAsync(openApiUri);

            var outPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "lupiraweb.client", "backend-openapi.json"));
            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
            File.WriteAllText(outPath, openApiContent);

            Console.WriteLine($"OpenAPI fetched from {openApiUri} and written to {outPath}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to fetch OpenAPI for frontend generation: " + ex.Message);
    }
}

await app.WaitForShutdownAsync();
internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
