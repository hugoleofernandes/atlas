using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application;

public interface IResult
{
    bool Success { get; }

    string? Error { get; }

    ErrorDefinition? ErrorDefinition { get; }

    object? GetValue();
}