using JasperFx;
using LupiraWeb.Admin.Server.Endpoints.Artifacts;
using LupiraWeb.Admin.Server.Endpoints.Goals;
using LupiraWeb.Admin.Server.Endpoints.Media;
using LupiraWeb.Admin.Server.Infrastructure.BlobStorage;
using LupiraWeb.Admin.Server.Observability;
using LupiraWeb.Domain;
using LupiraWeb.Domain.Infrastructure.BlobStorage;
using Marten;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

const string DefaultConnectionString =
    "Host=localhost;Port=5432;Database=lupiraweb;Username=lupira;Password=lupira";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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

builder.Services.AddSingleton<IBlobStorage, InMemoryBlobStorage>();

builder.Services.AddScoped<ArtifactsHandler>();
builder.Services.AddScoped<GoalsHandler>();
builder.Services.AddScoped<MediaHandler>();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

builder.AddLupiraObservability("lupira-admin");

var app = builder.Build();

if (args.Contains("--apply-schema"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
    await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
    Console.WriteLine("Schema applied.");
    return;
}

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });

app.MapArtifactsEndpoints();
app.MapGoalsEndpoints();
app.MapMediaEndpoints();

app.Run();

public partial class Program;
