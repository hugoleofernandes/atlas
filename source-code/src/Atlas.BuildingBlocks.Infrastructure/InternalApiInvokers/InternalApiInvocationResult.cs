namespace Atlas.BuildingBlocks.Application.InternalApiInvokers;

public sealed record InternalApiInvocationResult<TResponse>(
    bool IsSuccess,
    int? StatusCode,
    TResponse? Value,
    string? ErrorMessage,
    string? RawBody)
{
    public static InternalApiInvocationResult<TResponse> Success(
        int statusCode,
        TResponse? value,
        string? rawBody) =>
        new(true, statusCode, value, null, rawBody);

    public static InternalApiInvocationResult<TResponse> Failure(
        int? statusCode,
        string errorMessage,
        string? rawBody = null) =>
        new(false, statusCode, default, errorMessage, rawBody);
}
