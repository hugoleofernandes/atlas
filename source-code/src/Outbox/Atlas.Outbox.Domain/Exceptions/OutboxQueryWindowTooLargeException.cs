using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Outbox.Domain.Exceptions;

public sealed class OutboxQueryWindowTooLargeException : DomainException
{
    public const string ErrorCode = "outbox.query.window_too_large";

    public OutboxQueryWindowTooLargeException(TimeSpan maxWindow)
        : base(ErrorCode, ErrorCategory.Validation, $"Query window cannot exceed {maxWindow.TotalDays:0} days.") { }
}
