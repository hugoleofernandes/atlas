using System.Diagnostics;
using Atlas.SharedKernel.Application;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Atlas.BuildingBlocks.AspNetCore.Observability;

public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IRequestContextSetter requestContextSetter)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        requestContextSetter.SetCorrelationId(correlationId);

        // Enrich the root HTTP span so every trace in Tempo carries the correlation ID.
        Activity.Current?.SetTag("correlation_id", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    public static string? Get(HttpContext context)
        => context.Items[HeaderName]?.ToString();
}
