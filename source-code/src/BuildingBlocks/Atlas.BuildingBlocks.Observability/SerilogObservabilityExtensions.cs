using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.OpenTelemetry;

namespace Atlas.BuildingBlocks.Observability;

public static class SerilogObservabilityExtensions
{
    /// <summary>
    /// Adiciona o sink OTLP do Serilog para enviar logs ao Grafana Cloud (Loki).
    /// Não faz nada se <see cref="ObservabilitySettings.IsEnabled"/> for falso.
    /// </summary>
    public static LoggerConfiguration WriteToAtlasObservability(
        this LoggerConfiguration config,
        ObservabilitySettings settings,
        IHostEnvironment environment)
    {
        if (!settings.IsEnabled)
            return config;

        config.WriteTo.OpenTelemetry(o =>
        {
            o.Endpoint = $"{settings.Endpoint!.TrimEnd('/')}/v1/logs";
            o.Protocol = OtlpProtocol.HttpProtobuf;
            o.Headers  = new Dictionary<string, string>
            {
                ["Authorization"] = settings.ApiKey!
            };
            o.ResourceAttributes = new Dictionary<string, object>
            {
                ["service.name"]           = settings.ServiceName,
                ["service.version"]        = settings.ServiceVersion,
                ["deployment.environment"] = environment.EnvironmentName.ToLowerInvariant()
            };
        });

        return config;
    }
}
