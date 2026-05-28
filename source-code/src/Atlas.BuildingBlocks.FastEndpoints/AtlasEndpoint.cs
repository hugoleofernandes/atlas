using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.BuildingBlocks.FastEndpoints;

/// <summary>
/// Base FastEndpoints endpoint that exposes the same result-mapping helpers as
/// <see cref="AtlasController"/> — one method call handles both the success path
/// and the error path (Problem Details + correct HTTP status code).
///
/// Dependencies (e.g. IErrorMessageLocalizer) are resolved lazily via
/// <see cref="Endpoint{TRequest,TResponse}.Resolve{TService}"/> so derived classes
/// only declare the dependencies they actually need in their primary constructors.
/// </summary>
public abstract class AtlasEndpoint<TReq, TRes> : Endpoint<TReq, TRes>
    where TReq : notnull
{
    // -------------------------------------------------------------------------
    // Internals
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sends a Problem Details response for an <see cref="ErrorDefinition"/> that was not
    /// produced by a <c>Result&lt;T&gt;</c> (e.g. missing claims, config validation at the API boundary).
    /// </summary>
    protected Task SendErrorAsync(ErrorDefinition error) => SendProblemAsync(error);

    private Task SendProblemAsync(ErrorDefinition error)
    {
        var localizer  = Resolve<IErrorMessageLocalizer>();
        var statusCode = error.Category.ToHttpStatus();

        return Send.ResultAsync(Results.Problem(
            title:      localizer.Localize(error),
            detail:     error.FallbackMessage,
            statusCode: statusCode));
    }

    // -------------------------------------------------------------------------
    // Read — 200 OK
    // -------------------------------------------------------------------------

    /// <summary>200 OK — use when the result value IS the response (no mapping needed, e.g. queries).</summary>
    protected Task OkFromResultAsync(Result<TRes> result, CancellationToken ct = default)
    {
        if (!result.IsSuccess)
            return SendProblemAsync(result.ErrorDefinition!);

        return Send.OkAsync(result.Value!, ct);
    }

    /// <summary>200 OK — use when the result value needs to be mapped to the response type.</summary>
    protected Task OkFromResultAsync<TOutput>(
        Result<TOutput> result,
        Func<TOutput, TRes> map,
        CancellationToken ct = default)
    {
        if (!result.IsSuccess)
            return SendProblemAsync(result.ErrorDefinition!);

        return Send.OkAsync(map(result.Value!), ct);
    }

    // -------------------------------------------------------------------------
    // Create — 201 Created
    // -------------------------------------------------------------------------

    /// <summary>201 Created — maps the command output to the response type.</summary>
    protected Task CreatedFromResultAsync<TOutput>(
        Result<TOutput> result,
        Func<TOutput, TRes> map,
        CancellationToken ct = default)
    {
        if (!result.IsSuccess)
            return SendProblemAsync(result.ErrorDefinition!);

        return Send.ResponseAsync(map(result.Value!), 201, ct);
    }

    // -------------------------------------------------------------------------
    // Update — 200 OK with body / 204 No Content
    // -------------------------------------------------------------------------

    /// <summary>200 OK — use for synchronous updates that return an updated representation.</summary>
    protected Task UpdatedFromResultAsync<TOutput>(
        Result<TOutput> result,
        Func<TOutput, TRes> map,
        CancellationToken ct = default)
    {
        if (!result.IsSuccess)
            return SendProblemAsync(result.ErrorDefinition!);

        return Send.OkAsync(map(result.Value!), ct);
    }

    /// <summary>204 No Content — use for synchronous updates that return no body.</summary>
    protected Task UpdatedNoContentFromResultAsync<T>(Result<T> result, CancellationToken ct = default)
    {
        if (!result.IsSuccess)
            return SendProblemAsync(result.ErrorDefinition!);

        return Send.NoContentAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Delete — 204 No Content / 200 OK with body
    // -------------------------------------------------------------------------

    /// <summary>204 No Content — use for synchronous deletes.</summary>
    protected Task DeletedFromResultAsync<T>(Result<T> result, CancellationToken ct = default)
    {
        if (!result.IsSuccess)
            return SendProblemAsync(result.ErrorDefinition!);

        return Send.NoContentAsync(ct);
    }

    /// <summary>200 OK — use for deletes that return details about the deleted resource.</summary>
    protected Task DeletedWithBodyFromResultAsync<TOutput>(
        Result<TOutput> result,
        Func<TOutput, TRes> map,
        CancellationToken ct = default)
    {
        if (!result.IsSuccess)
            return SendProblemAsync(result.ErrorDefinition!);

        return Send.OkAsync(map(result.Value!), ct);
    }
}
