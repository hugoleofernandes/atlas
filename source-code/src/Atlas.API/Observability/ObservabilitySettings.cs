namespace Atlas.API.Observability;

public sealed class ObservabilitySettings
{
    public const string SectionName = "OpenTelemetry";

    public string ServiceName { get; init; } = "atlas-api";
    public string ServiceVersion { get; init; } = "1.0.0";

    /// <summary>
    /// OTLP endpoint base URL.
    /// Example: https://otlp-gateway-prod-us-central-0.grafana.net/otlp
    /// Set via user secrets or environment variable: OpenTelemetry__Endpoint
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Authorization header value for Grafana Cloud.
    /// Format: "Basic &lt;base64(instanceId:apiToken)&gt;"
    /// Set via user secrets or environment variable: OpenTelemetry__ApiKey
    /// </summary>
    public string? ApiKey { get; init; }

    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        !string.IsNullOrWhiteSpace(ApiKey);
}
