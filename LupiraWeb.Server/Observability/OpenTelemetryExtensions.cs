using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LupiraWeb.Server.Observability;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddLupiraObservability(this WebApplicationBuilder builder)
    {
        const string serviceName = "LupiraWeb.Server";
        var isDev = builder.Environment.IsDevelopment();
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation();
                if (isDev) t.AddConsoleExporter();
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            })
            .WithMetrics(m =>
            {
                m.AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation()
                 .AddRuntimeInstrumentation();
                if (isDev) m.AddConsoleExporter();
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    m.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            });

        builder.Logging.AddOpenTelemetry(o =>
        {
            o.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
            if (isDev) o.AddConsoleExporter();
            if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                o.AddOtlpExporter(e => e.Endpoint = new Uri(otlpEndpoint));
        });

        return builder;
    }
}
