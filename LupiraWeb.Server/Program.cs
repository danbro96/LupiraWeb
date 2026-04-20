using LupiraWeb.Server.Data;
using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Domain;
using LupiraWeb.Server.Endpoints.Artifacts;
using LupiraWeb.Server.Endpoints.Experiences;
using LupiraWeb.Server.Endpoints.Goals;
using LupiraWeb.Server.Endpoints.Media;
using LupiraWeb.Server.Endpoints.Resume;
using LupiraWeb.Server.Endpoints.Skills;
using LupiraWeb.Domain.Infrastructure.BlobStorage;
using LupiraWeb.Server.Infrastructure.BlobStorage;
using LupiraWeb.Server.Observability;
using JasperFx;
using Marten;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

const string DefaultConnectionString =
    "Host=localhost;Port=5432;Database=lupiraweb;Username=lupira;Password=lupira";

builder.Services.AddMarten(sp =>
{
    var opts = new StoreOptions();
    opts.Connection(sp.GetRequiredService<IConfiguration>().GetConnectionString("AppDb")
        ?? DefaultConnectionString);
    opts.UseSystemTextJsonForSerialization();
    opts.DatabaseSchemaName = "marten";
    opts.AutoCreateSchemaObjects = builder.Environment.IsProduction()
        ? AutoCreate.None
        : AutoCreate.CreateOrUpdate;

    opts.UseLupiraProjections();

    return opts;
}).UseLightweightSessions();

builder.Services.AddScoped<IMyInfoRepository, MyInfoRepository>();
builder.Services.AddScoped<IEngagementRepository, EngagementRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();

builder.Services.AddSingleton<IBlobStorage, InMemoryBlobStorage>();

builder.Services.AddScoped<ResumeHandler>();
builder.Services.AddScoped<LupiraWeb.Server.Endpoints.Skills.SkillsHandler>();
builder.Services.AddScoped<MediaHandler>();
builder.Services.AddScoped<ArtifactsHandler>();
builder.Services.AddScoped<GoalsHandler>();
builder.Services.AddScoped<ExperiencesHandler>();

builder.Services.AddHealthChecks()
    .AddCheck<MartenHealthCheck>("marten", tags: new[] { "ready" });

builder.AddLupiraObservability();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DbInitializer.InitializeAsync(app.Services);
}

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });

app.MapResumeEndpoints();
app.MapSkillsEndpoints();
app.MapMediaEndpoints();
app.MapArtifactsEndpoints();
app.MapGoalsEndpoints();
app.MapExperiencesEndpoints();

app.Run();

public partial class Program;
