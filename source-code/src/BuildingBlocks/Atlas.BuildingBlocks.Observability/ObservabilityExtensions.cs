using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Atlas.BuildingBlocks.Observability;

public static class ObservabilityExtensions
{
    /// <summary>
    /// Configura OpenTelemetry (traces + metrics) com exportador OTLP para Grafana Cloud.
    /// Inclui instrumentações comuns a qualquer host (.NET Runtime, Process, EF Core, HttpClient).
    ///
    /// Para adicionar instrumentações específicas do host (ex: AspNetCore na API),
    /// use os parâmetros opcionais <paramref name="configureTracing"/> e <paramref name="configureMetrics"/>.
    ///
    /// Se <c>OpenTelemetry:Endpoint</c> não estiver configurado, o método retorna sem registrar nada.
    /// </summary>
    public static IServiceCollection AddAtlasObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Action<TracerProviderBuilder>? configureTracing = null,
        Action<MeterProviderBuilder>?  configureMetrics = null)
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
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource("Atlas")
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(o => ConfigureOtlp(o, settings, "traces"));

                // Instrumentações específicas do host (ex: AspNetCore na API)
                configureTracing?.Invoke(tracing);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter("Microsoft.EntityFrameworkCore")
                    .AddMeter("Atlas")
                    .AddOtlpExporter(o => ConfigureOtlp(o, settings, "metrics"));

                // Instrumentações específicas do host (ex: AspNetCore na API)
                configureMetrics?.Invoke(metrics);
            });

        return services;
    }

    private static void ConfigureOtlp(
        OtlpExporterOptions options,
        ObservabilitySettings settings,
        string signal)
    {
        options.Endpoint = new Uri($"{settings.Endpoint!.TrimEnd('/')}/v1/{signal}");
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
        options.Headers  = $"Authorization={settings.ApiKey}";
    }
}
