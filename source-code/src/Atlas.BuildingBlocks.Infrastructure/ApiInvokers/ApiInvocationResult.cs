namespace Atlas.BuildingBlocks.Application.ApiInvokers;

public sealed record ApiInvocationResult(
    bool IsSuccess,
    int? StatusCode,
    string? ErrorMessage)
{
    public static ApiInvocationResult Success(int statusCode)
        => new(true, statusCode, null);

    public static ApiInvocationResult Failure(int? statusCode, string errorMessage)
        => new(false, statusCode, errorMessage);
}
