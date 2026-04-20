using LupiraWeb.Server.Data;
using LupiraWeb.Server.Data.Repositories;
using LupiraWeb.Server.Endpoints.Resume;
using LupiraWeb.Server.Observability;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("AppDb")
    ?? "Data Source=./app_data/lupiraweb.db;Cache=Shared";

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));

builder.Services.AddScoped<IMyInfoRepository, MyInfoRepository>();
builder.Services.AddScoped<IEmploymentRepository, EmploymentRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();

builder.Services.AddScoped<ResumeHandler>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("appdb", tags: new[] { "ready" });

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

app.Run();

public partial class Program;
