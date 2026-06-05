using System.Net.Http;

namespace Atlas.BuildingBlocks.Application.InternalApiInvokers;

public sealed record InternalApiInvocationRequest(
    string Name,
    HttpMethod Method,
    Uri Url,
    object? Payload = null,
    string? CorrelationId = null,
    string? TraceParent = null);
