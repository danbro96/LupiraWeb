using System.Text.Json.Serialization;
using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Endpoints;
using LupiraWeb.Server.Endpoints.Artifacts;
using LupiraWeb.Server.Endpoints.Demos.Chat;
using LupiraWeb.Server.Endpoints.Demos.TextToSpeech;
using LupiraWeb.Server.Endpoints.Demos.Vision;
using LupiraWeb.Server.Endpoints.Experiences;
using LupiraWeb.Server.Endpoints.Media;
using LupiraWeb.Server.Endpoints.Resume;
using LupiraWeb.Server.Endpoints.Skills;
using LupiraWeb.Server.Integration.CareerApi;
using LupiraWeb.Server.Integration.CareerApi.Auth;
using LupiraWeb.Server.Integration.CareerApi.Repositories;
using LupiraWeb.Server.Observability;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Emit enums as their names (not ints) in responses and the generated OpenAPI doc → typed string unions in the client.
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// --- LupiraCareerApi integration: the career/résumé data now lives in CareerApi, read over HTTP.
//     This replaces the local Marten/Postgres store entirely. ---
// BaseUrl ships a localhost dev-default in appsettings.json (so local runs and the build-time OpenAPI
// generation that boots the host work without extra config); production overrides it via CareerApi__BaseUrl.
// Used only to construct the HttpClient — not contacted at startup.
var careerApiBaseUrl = builder.Configuration["CareerApi:BaseUrl"]
    ?? throw new InvalidOperationException("CareerApi:BaseUrl is required");
var careerApiTimeout = TimeSpan.FromSeconds(
    builder.Configuration.GetValue<int?>("CareerApi:TimeoutSeconds") ?? 30);

// Auth seam: development/Testing present CareerApi's X-Dev-User header; production mints a machine
// (client-credentials) bearer token from Authentik and refreshes it.
if (builder.Environment.IsProduction())
    builder.Services.AddSingleton<ICareerApiTokenProvider, ClientCredentialsTokenProvider>();
else
    builder.Services.AddSingleton<ICareerApiTokenProvider, DevUserTokenProvider>();

builder.Services.AddTransient<CareerApiAuthHandler>();

// Token-mint client for the client-credentials grant (production). No auth handler; short timeout.
builder.Services.AddHttpClient(ClientCredentialsTokenProvider.TokenClientName,
    c => c.Timeout = TimeSpan.FromSeconds(10));

builder.Services.AddHttpClient<ICareerApiClient, CareerApiClient>(c =>
{
    c.BaseAddress = new Uri(careerApiBaseUrl);
    c.Timeout = careerApiTimeout;
}).AddHttpMessageHandler<CareerApiAuthHandler>();

// Unauthenticated probe client for readiness (CareerApi /livez): no auth handler, short timeout.
builder.Services.AddHttpClient(CareerApiHealthCheck.HealthClientName, c =>
{
    c.BaseAddress = new Uri(careerApiBaseUrl);
    c.Timeout = TimeSpan.FromSeconds(3);
});

// Repositories read from CareerApi over HTTP; the interfaces are unchanged so the résumé handler and its
// unit tests are untouched.
builder.Services.AddScoped<IMyInfoRepository, MyInfoRepository>();
builder.Services.AddScoped<IEngagementRepository, EngagementRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();

builder.Services.AddScoped<ResumeHandler>();
builder.Services.AddScoped<LupiraWeb.Server.Endpoints.Skills.SkillsHandler>();
builder.Services.AddScoped<MediaHandler>();
builder.Services.AddScoped<ArtifactsHandler>();
builder.Services.AddScoped<ExperiencesHandler>();

builder.Services.AddHttpClient<ChatHandler>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Demos:Chat:BaseUrl"]
        ?? throw new InvalidOperationException("Demos:Chat:BaseUrl is required"));
    var apiKey = builder.Configuration["Demos:Chat:ApiKey"];
    if (!string.IsNullOrEmpty(apiKey))
        c.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
    c.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddHttpClient<TextToSpeechHandler>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Demos:TextToSpeech:BaseUrl"]
        ?? throw new InvalidOperationException("Demos:TextToSpeech:BaseUrl is required"));
    var apiKey = builder.Configuration["Demos:TextToSpeech:ApiKey"];
    if (!string.IsNullOrEmpty(apiKey))
        c.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    c.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddHttpClient<VisionHandler>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Demos:Vision:BaseUrl"]
        ?? throw new InvalidOperationException("Demos:Vision:BaseUrl is required"));
    var apiKey = builder.Configuration["Demos:Vision:ApiKey"];
    if (!string.IsNullOrEmpty(apiKey))
        c.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    c.Timeout = TimeSpan.FromSeconds(60);
});

// Liveness (/livez) + readiness (/readyz, pings CareerApi) probes.
builder.Services.AddAppHealthChecks();

builder.AddLupiraObservability("lupira-web");

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapAppHealthChecks(app.Environment);

app.MapResumeEndpoints();
app.MapSkillsEndpoints();
app.MapMediaEndpoints();
app.MapArtifactsEndpoints();
app.MapExperiencesEndpoints();

app.MapChatEndpoints();
app.MapTextToSpeechEndpoints();
app.MapVisionEndpoints();

app.Run();

public partial class Program;
