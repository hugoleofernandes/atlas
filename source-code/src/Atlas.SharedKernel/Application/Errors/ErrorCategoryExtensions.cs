namespace Atlas.SharedKernel.Application.Errors;

/// <summary>
/// Maps an <see cref="ErrorCategory"/> to its canonical HTTP status code.
/// Single source of truth — used by GlobalExceptionMiddleware, HttpResultMapper, and FastEndpoints.
/// </summary>
public static class ErrorCategoryExtensions
{
    public static int ToHttpStatus(this ErrorCategory category)
        => category switch
        {
            ErrorCategory.Validation   => 400,
            ErrorCategory.Conflict     => 409,
            ErrorCategory.NotFound     => 404,
            ErrorCategory.Business     => 422,
            ErrorCategory.Unauthorized => 401,
            ErrorCategory.Unexpected   => 500,
            _                          => 500
        };
}
