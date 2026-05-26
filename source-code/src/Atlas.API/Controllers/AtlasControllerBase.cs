using Atlas.API.Errors;
using Atlas.API.Models;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.API.Controllers;

/// <summary>
/// Base controller that exposes explicit helpers for converting application results
/// into the public HTTP response contract.
/// </summary>
public abstract class AtlasControllerBase : ControllerBase
{
    protected ErrorMessageLocalizer ErrorLocalizer { get; }
    private readonly IHttpResultMapper _resultMapper;

    protected AtlasControllerBase(
        ErrorMessageLocalizer errorLocalizer,
        IHttpResultMapper resultMapper)
    {
        ErrorLocalizer = errorLocalizer;
        _resultMapper = resultMapper;
    }

    protected IActionResult OkFromResult<T>(Result<T> result)
        => _resultMapper.ToOkResult(result);

    /// <summary>
    /// Returns 201 Created with a response body, without a Location header.
    /// Use when the command creates a resource but there is no GET-by-id endpoint yet.
    /// </summary>
    protected IActionResult CreatedFromResult<TOutput, TResponse>(
        Result<TOutput> result)
        where TResponse : IResponseFrom<TOutput, TResponse>
        => _resultMapper.ToCreatedResult(result, TResponse.From);

    /// <summary>
    /// Returns 202 Accepted with an optional response body.
    /// Use for create requests accepted for asynchronous processing, where the resource may not exist yet.
    /// </summary>
    protected IActionResult CreateAcceptedFromResult<TOutput, TResponse>(
        Result<TOutput> result)
        where TResponse : IResponseFrom<TOutput, TResponse>
        => _resultMapper.ToCreateAcceptedResult(result, TResponse.From);

    /// <summary>
    /// Returns 200 OK with the updated representation in the response body.
    /// Use for synchronous updates that return a DTO.
    /// </summary>
    protected IActionResult UpdatedFromResult<TOutput, TResponse>(
        Result<TOutput> result)
        where TResponse : IResponseFrom<TOutput, TResponse>
        => _resultMapper.ToUpdatedResult(result, TResponse.From);

    /// <summary>
    /// Returns 204 No Content.
    /// Use for synchronous updates that complete successfully but do not need to return a body.
    /// </summary>
    protected IActionResult UpdatedNoContentFromResult<T>(Result<T> result)
        => _resultMapper.ToUpdatedNoContentResult(result);

    /// <summary>
    /// Returns 202 Accepted with an optional response body.
    /// Use for updates accepted for asynchronous processing, where the update may not be complete yet.
    /// </summary>
    protected IActionResult UpdateAcceptedFromResult<TOutput, TResponse>(
        Result<TOutput> result)
        where TResponse : IResponseFrom<TOutput, TResponse>
        => _resultMapper.ToUpdateAcceptedResult(result, TResponse.From);

    /// <summary>
    /// Returns 204 No Content.
    /// Use for synchronous deletes that complete successfully and do not need to return a body.
    /// </summary>
    protected IActionResult DeletedFromResult<T>(Result<T> result)
        => _resultMapper.ToDeletedResult(result);

    /// <summary>
    /// Returns 200 OK with a response body.
    /// Use for synchronous deletes that return details about the deleted resource or action.
    /// </summary>
    protected IActionResult DeletedWithBodyFromResult<TOutput, TResponse>(
        Result<TOutput> result)
        where TResponse : IResponseFrom<TOutput, TResponse>
        => _resultMapper.ToDeletedWithBodyResult(result, TResponse.From);

    /// <summary>
    /// Returns 202 Accepted with an optional response body.
    /// Use for deletes accepted for asynchronous processing, where deletion may not be complete yet.
    /// </summary>
    protected IActionResult DeleteAcceptedFromResult<TOutput, TResponse>(
        Result<TOutput> result)
        where TResponse : IResponseFrom<TOutput, TResponse>
        => _resultMapper.ToDeleteAcceptedResult(result, TResponse.From);

    /// <summary>
    /// Returns 201 Created with a Location header pointing to the created resource.
    /// Use when the command creates a resource and a GET-by-id endpoint exists.
    /// </summary>
    protected IActionResult CreatedAtActionFromResult<TOutput, TResponse>(
        Result<TOutput> result,
        string actionName,
        Func<TOutput, object?> routeValues)
        where TResponse : IResponseFrom<TOutput, TResponse>
        => _resultMapper.ToCreatedAtActionResult(this, result, actionName, routeValues, TResponse.From);

    protected ObjectResult ErrorResult(ErrorDefinition error)
        => _resultMapper.ToProblemResult(error);
}
