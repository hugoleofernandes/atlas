namespace Atlas.BuildingBlocks.Observability;

public sealed class ObservabilitySettings
{
    public const string SectionName = "OpenTelemetry";

    /// <summary>
    /// Nome do serviço reportado no Grafana (service.name).
    /// Exemplo: "atlas-api", "atlas-outbox-worker"
    /// </summary>
    public string ServiceName { get; init; } = "atlas";

    public string ServiceVersion { get; init; } = "1.0.0";

    /// <summary>
    /// OTLP endpoint base URL.
    /// Exemplo: https://otlp-gateway-prod-us-central-0.grafana.net/otlp
    /// Configure via user secrets ou variável de ambiente: OpenTelemetry__Endpoint
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Header de autorização para Grafana Cloud.
    /// Formato: "Basic &lt;base64(instanceId:apiToken)&gt;"
    /// Configure via user secrets ou variável de ambiente: OpenTelemetry__ApiKey
    /// </summary>
    public string? ApiKey { get; init; }

    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        !string.IsNullOrWhiteSpace(ApiKey);
}
