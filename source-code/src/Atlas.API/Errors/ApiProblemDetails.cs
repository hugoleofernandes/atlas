using Microsoft.AspNetCore.Mvc;

namespace Atlas.API.Errors;

public sealed class ApiProblemDetails : ProblemDetails
{
    public void AddMetadata(
        string? errorCode,
        string? correlationId,
        string? traceId)
    {
        if (!string.IsNullOrWhiteSpace(errorCode))
            Extensions["errorCode"] = errorCode;

        if (!string.IsNullOrWhiteSpace(correlationId))
            Extensions["correlationId"] = correlationId;

        if (!string.IsNullOrWhiteSpace(traceId))
            Extensions["traceId"] = traceId;

        Extensions["timestamp"] = DateTime.UtcNow;
    }
}