using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Atlas.API.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddAtlasObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var settings = configuration
            .GetSection(ObservabilitySettings.SectionName)
            .Get<ObservabilitySettings>() ?? new ObservabilitySettings();

        if (!settings.IsEnabled)
            return services;

        services
            .AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(settings.ServiceName, serviceVersion: settings.ServiceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment.EnvironmentName.ToLowerInvariant()
                }))
            .WithTracing(tracing => tracing
                .AddSource("Atlas")
                .AddAspNetCoreInstrumentation(o =>
                {
                    // Ignora health checks para não poluir os traces
                    o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                    o.RecordException = true;
                })
                .AddHttpClientInstrumentation()
                // Instrumentação EF Core — captura queries no trace
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(o => ConfigureOtlp(o, settings, "traces")))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(o => ConfigureOtlp(o, settings, "metrics")));

        return services;
    }

    private static void ConfigureOtlp(
        OtlpExporterOptions options,
        ObservabilitySettings settings,
        string signal)
    {
        options.Endpoint = new Uri($"{settings.Endpoint!.TrimEnd('/')}/v1/{signal}");
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
        options.Headers = $"Authorization={settings.ApiKey}";
    }
}
