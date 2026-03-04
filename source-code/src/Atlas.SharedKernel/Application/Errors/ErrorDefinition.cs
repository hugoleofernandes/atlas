namespace Atlas.SharedKernel.Application.Errors;

public sealed record ErrorDefinition(
    string Code,
    string DefaultMessage,
    ErrorCategory Category
);