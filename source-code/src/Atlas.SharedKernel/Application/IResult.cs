namespace Atlas.SharedKernel.Application;

public interface IResult
{
    bool Success { get; }
    string? Error { get; }
    string? ErrorCode { get; }
}