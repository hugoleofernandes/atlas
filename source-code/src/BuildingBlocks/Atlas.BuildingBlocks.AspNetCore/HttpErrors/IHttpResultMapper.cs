using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.BuildingBlocks.AspNetCore.HttpErrors;

public interface IHttpResultMapper
{
    /// <summary>
    /// Maps a successful result to 200 OK with the result value as the response body.
    /// Use for reads and command responses that do not create a new resource.
    /// </summary>
    IActionResult ToOkResult<T>(Result<T> result);

    /// <summary>
    /// Maps a successful result to 201 Created with a response body, without a Location header.
    /// Use when a resource is created but there is no GET endpoint for the created resource yet.
    /// </summary>
    IActionResult ToCreatedResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map);

    /// <summary>
    /// Maps a successful result to 202 Accepted with an optional response body.
    /// Use for create requests accepted for asynchronous processing, where the resource may not exist yet.
    /// </summary>
    IActionResult ToCreateAcceptedResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map);

    /// <summary>
    /// Maps a successful result to 200 OK with the updated representation as the response body.
    /// Use for synchronous updates that return a DTO.
    /// </summary>
    IActionResult ToUpdatedResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map);

    /// <summary>
    /// Maps a successful result to 204 No Content.
    /// Use for synchronous updates that complete successfully but do not need to return a body.
    /// </summary>
    IActionResult ToUpdatedNoContentResult<T>(Result<T> result);

    /// <summary>
    /// Maps a successful result to 202 Accepted with an optional response body.
    /// Use for updates accepted for asynchronous processing, where the update may not be complete yet.
    /// </summary>
    IActionResult ToUpdateAcceptedResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map);

    /// <summary>
    /// Maps a successful result to 204 No Content.
    /// Use for synchronous deletes that complete successfully and do not need to return a body.
    /// </summary>
    IActionResult ToDeletedResult<T>(Result<T> result);

    /// <summary>
    /// Maps a successful result to 200 OK with a response body.
    /// Use for synchronous deletes that return details about the deleted resource or action.
    /// </summary>
    IActionResult ToDeletedWithBodyResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map);

    /// <summary>
    /// Maps a successful result to 202 Accepted with an optional response body.
    /// Use for deletes accepted for asynchronous processing, where deletion may not be complete yet.
    /// </summary>
    IActionResult ToDeleteAcceptedResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map);

    /// <summary>
    /// Maps a successful result to 201 Created with a Location header generated from an action.
    /// Use when a resource is created and a GET endpoint exists for the created resource.
    /// </summary>
    IActionResult ToCreatedAtActionResult<TOutput, TResponse>(
        ControllerBase controller,
        Result<TOutput> result,
        string actionName,
        Func<TOutput, object?> routeValues,
        Func<TOutput, TResponse> map);

    ObjectResult ToProblemResult(ErrorDefinition error, string? detail = null);
}
