using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LupiraWeb.Server.Observability;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddLupiraObservability(this WebApplicationBuilder builder, string serviceName)
    {
        var isDev = builder.Environment.IsDevelopment();
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
        var serviceVersion = typeof(OpenTelemetryExtensions).Assembly.GetName().Version?.ToString() ?? "0.0.0";

        // Don't set Endpoint in code: it disables the /v1/{signal} path append under http/protobuf. Let the SDK read OTEL_* env.
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName, serviceVersion: serviceVersion))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation(o => o.RecordException = true)
                 .AddHttpClientInstrumentation();
                if (isDev) t.AddConsoleExporter();
                if (!string.IsNullOrWhiteSpace(otlpEndpoint)) t.AddOtlpExporter();
            })
            .WithMetrics(m =>
            {
                m.AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation()
                 .AddRuntimeInstrumentation();
                if (isDev) m.AddConsoleExporter();
                if (!string.IsNullOrWhiteSpace(otlpEndpoint)) m.AddOtlpExporter();
            });

        builder.Logging.AddOpenTelemetry(o =>
        {
            o.IncludeFormattedMessage = true;
            o.IncludeScopes = true;
            o.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion));
            if (isDev) o.AddConsoleExporter();
            if (!string.IsNullOrWhiteSpace(otlpEndpoint)) o.AddOtlpExporter();
        });

        return builder;
    }
}
