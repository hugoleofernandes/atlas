using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Outbox.Domain.Exceptions;

public sealed class OutboxMessageNotDeadLetteredException : DomainException
{
    public const string ErrorCode = "outbox.message.not_dead_lettered";

    public OutboxMessageNotDeadLetteredException(Guid id)
        : base(ErrorCode, ErrorCategory.Business, $"Outbox message '{id}' is not dead-lettered and cannot be resubmitted.") { }
}
