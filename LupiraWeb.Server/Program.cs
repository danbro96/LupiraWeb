using LupiraWeb.Server.Data;
using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Domain;
using LupiraWeb.Server.Endpoints.Artifacts;
using LupiraWeb.Server.Endpoints.Experiences;
using LupiraWeb.Server.Endpoints.Goals;
using LupiraWeb.Server.Endpoints.Media;
using LupiraWeb.Server.Endpoints.Resume;
using LupiraWeb.Server.Endpoints.Skills;
using LupiraWeb.Server.Infrastructure.BlobStorage;
using LupiraWeb.Server.Observability;
using JasperFx;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
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

    opts.Projections.Snapshot<Skill>(SnapshotLifecycle.Inline);
    opts.Projections.Snapshot<Engagement>(SnapshotLifecycle.Inline);
    opts.Projections.Snapshot<Project>(SnapshotLifecycle.Inline);
    opts.Projections.Snapshot<MediaAsset>(SnapshotLifecycle.Inline);
    opts.Projections.Snapshot<Artifact>(SnapshotLifecycle.Inline);
    opts.Projections.Snapshot<Goal>(SnapshotLifecycle.Inline);
    opts.Projections.Add<EngagementTitleHistoryProjection>(ProjectionLifecycle.Inline);
    opts.Projections.Add<SkillTimelineProjection>(ProjectionLifecycle.Inline);
    opts.Projections.Add<SkillMaturityProjection>(ProjectionLifecycle.Inline);
    opts.Projections.Add<SkillAdjacencyProjection>(ProjectionLifecycle.Inline);
    opts.Projections.Add<ProjectMediaProjection>(ProjectionLifecycle.Inline);
    opts.Projections.Add<SkillMediaGalleryProjection>(ProjectionLifecycle.Inline);
    opts.Projections.Add<ProjectArtifactsProjection>(ProjectionLifecycle.Inline);
    opts.Projections.Add<SkillArtifactsProjection>(ProjectionLifecycle.Inline);
    opts.Projections.Add<EngagementArtifactsProjection>(ProjectionLifecycle.Inline);
    opts.Projections.Add<SkillGoalIndexProjection>(ProjectionLifecycle.Inline);
    opts.Projections.Add<ExperienceProjection>(ProjectionLifecycle.Inline);

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
