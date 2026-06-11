using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Outbox.Domain.Exceptions;

public sealed class OutboxMessageNotFoundException : DomainException
{
    public const string ErrorCode = "outbox.message.not_found";

    public OutboxMessageNotFoundException(Guid id)
        : base(ErrorCode, ErrorCategory.NotFound, $"Outbox message '{id}' was not found.") { }
}
