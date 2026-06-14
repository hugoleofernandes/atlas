using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Outbox.Domain.Exceptions;

public sealed class OutboxQueryWindowInvalidException : DomainException
{
    public const string ErrorCode = "outbox.query.window_invalid";

    public OutboxQueryWindowInvalidException(DateTime from, DateTime to)
        : base(ErrorCode, ErrorCategory.Validation, $"'from' ({from:O}) must be earlier than 'to' ({to:O}).") { }
}
