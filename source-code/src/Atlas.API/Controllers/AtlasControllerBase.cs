using Atlas.API.Errors;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.API.Controllers;

/// <summary>
/// Base controller that provides a consistent error-response helper for all Atlas API controllers.
/// Uses the same ApiProblemDetails shape as GlobalExceptionMiddleware, ensuring every error
/// response — whether caught by middleware or returned explicitly — looks identical to the client.
/// </summary>
public abstract class AtlasControllerBase : ControllerBase
{
    protected ErrorMessageLocalizer ErrorLocalizer { get; }

    protected AtlasControllerBase(ErrorMessageLocalizer errorLocalizer)
    {
        ErrorLocalizer = errorLocalizer;
    }

    /// <summary>
    /// Returns a structured <see cref="ApiProblemDetails"/> response whose HTTP status code
    /// is derived from <see cref="ErrorDefinition.Category"/>, matching the same mapping used
    /// by <see cref="GlobalExceptionMiddleware"/>.
    /// </summary>
    protected ObjectResult ErrorResult(ErrorDefinition error)
    {
        var status = MapCategory(error.Category);

        var problem = new ApiProblemDetails
        {
            Title = ErrorLocalizer.Localize(error),
            Status = status,
            Type = $"https://docs.atlas/errors/{error.Code}"
        };

        problem.AddMetadata(error.Code, correlationId: null, traceId: null);

        return StatusCode(status, problem);
    }

    private static int MapCategory(ErrorCategory category) => category switch
    {
        ErrorCategory.Validation   => StatusCodes.Status400BadRequest,
        ErrorCategory.Business     => StatusCodes.Status422UnprocessableEntity,
        ErrorCategory.Conflict     => StatusCodes.Status409Conflict,
        ErrorCategory.NotFound     => StatusCodes.Status404NotFound,
        ErrorCategory.Unauthorized => StatusCodes.Status401Unauthorized,
        _                          => StatusCodes.Status500InternalServerError
    };
}
