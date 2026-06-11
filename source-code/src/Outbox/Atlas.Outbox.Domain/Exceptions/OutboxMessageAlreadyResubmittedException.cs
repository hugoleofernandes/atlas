using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Outbox.Domain.Exceptions;

public sealed class OutboxMessageAlreadyResubmittedException : DomainException
{
    public const string ErrorCode = "outbox.message.already_resubmitted";

    public OutboxMessageAlreadyResubmittedException(Guid id)
        : base(ErrorCode, ErrorCategory.Conflict, $"Outbox message '{id}' already has a child attempt.") { }
}
