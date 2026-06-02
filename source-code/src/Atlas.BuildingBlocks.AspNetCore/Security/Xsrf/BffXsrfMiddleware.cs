using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Observability;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Atlas.BuildingBlocks.AspNetCore.Security.Xsrf;

public sealed class BffXsrfMiddleware(
    RequestDelegate next,
    IAntiforgery antiforgery,
    IOptions<BffXsrfOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldValidate(context))
        {
            await next(context);
            return;
        }

        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            await WriteXsrfProblemAsync(context);
            return;
        }

        await next(context);
    }

    private bool ShouldValidate(HttpContext context)
    {
        if (HttpMethods.IsGet(context.Request.Method)
            || HttpMethods.IsHead(context.Request.Method)
            || HttpMethods.IsOptions(context.Request.Method)
            || HttpMethods.IsTrace(context.Request.Method))
            return false;

        if (!context.Request.Path.StartsWithSegments(options.Value.BffPathPrefix))
            return false;

        return !options.Value.ExcludedPaths.Contains(context.Request.Path);
    }

    private static Task WriteXsrfProblemAsync(HttpContext context)
    {
        var problem = new ApiProblemDetails
        {
            Type = $"https://docs.atlas/errors/{BffXsrfDefaults.ErrorCode}",
            Title = "Invalid or missing XSRF token.",
            Status = StatusCodes.Status403Forbidden,
        };

        problem.AddMetadata(
            BffXsrfDefaults.ErrorCode,
            CorrelationIdMiddleware.Get(context),
            TraceContextHelper.GetTraceId());

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsJsonAsync(problem);
    }
}
