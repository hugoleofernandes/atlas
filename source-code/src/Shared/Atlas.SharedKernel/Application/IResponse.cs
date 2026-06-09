using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application;

public interface IResponse
{
    bool IsSuccess { get; }
    string? Error { get; }
    ErrorDefinition? ErrorDefinition { get; }
    object? GetValue();
}
