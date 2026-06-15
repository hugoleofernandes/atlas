using Atlas.Outbox.Domain.Exceptions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.ResubmitDeadLetter;

public sealed class ResubmitDeadLetterCommandHandler(
    IOutboxWorkerRepository repository,
    IRequestContext requestContext,
    IUnitOfWork unitOfWork)
    : IIdentityResubmitDeadLetterCommandHandler,
      IStaffResubmitDeadLetterCommandHandler
{
    public IUnitOfWork UnitOfWork => unitOfWork;

    public async Task<ResubmitDeadLetterOutput> ExecuteAsync(
        ResubmitDeadLetterCommand command,
        CancellationToken ct)
    {
        var message = await repository.GetByIdAsync(command.MessageId, ct)
            ?? throw new OutboxMessageNotFoundException(command.MessageId);

        var hasReplayChild = await repository.HasChildAsync(command.MessageId, ct);

        if (!OutboxMessage.CanBeResubmitted(message.IsDeadLettered, hasReplayChild))
        {
            if (!message.IsDeadLettered)
                throw new OutboxMessageNotDeadLetteredException(command.MessageId);

            throw new OutboxMessageAlreadyResubmittedException(command.MessageId);
        }

        var userId = requestContext.UserId
            ?? throw new InvalidOperationException("User context is not available for resubmission authorship.");
        var email = requestContext.UserEmail
            ?? throw new InvalidOperationException("User email is not available in context.");

        var replay = message.CreateResubmissionAttempt(userId, email);
        await repository.AddRetryAsync(replay, ct);

        return new ResubmitDeadLetterOutput(replay.Id);
    }
}
