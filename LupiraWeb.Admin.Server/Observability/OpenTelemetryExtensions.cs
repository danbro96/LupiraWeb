using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LupiraWeb.Admin.Server.Observability;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddLupiraObservability(this WebApplicationBuilder builder, string serviceName)
    {
        var isDev = builder.Environment.IsDevelopment();
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
        var serviceVersion = typeof(OpenTelemetryExtensions).Assembly.GetName().Version?.ToString() ?? "0.0.0";

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName, serviceVersion: serviceVersion))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation(o => o.RecordException = true)
                 .AddHttpClientInstrumentation()
                 .AddSource("Marten");
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
            o.IncludeFormattedMessage = true;
            o.IncludeScopes = true;
            o.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion));
            if (isDev) o.AddConsoleExporter();
            if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                o.AddOtlpExporter(e => e.Endpoint = new Uri(otlpEndpoint));
        });

        return builder;
    }
}
