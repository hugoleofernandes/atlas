using Atlas.Outbox.Domain.Exceptions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.ResubmitDeadLetter;

public sealed class ResubmitDeadLetterCommandHandler(
    IOutboxWorkerRepository repository,
    IRequestContext requestContext,
    IUnitOfWork unitOfWork)
    : IResubmitDeadLetterCommandHandler
{
    public IUnitOfWork UnitOfWork => unitOfWork;

    public async Task<ResubmitDeadLetterOutput> ExecuteAsync(
        ResubmitDeadLetterCommand command,
        CancellationToken ct)
    {
        var message = await repository.GetByIdAsync(command.MessageId, ct)
            ?? throw new OutboxMessageNotFoundException(command.MessageId);

        if (!message.IsDeadLettered)
            throw new OutboxMessageNotDeadLetteredException(command.MessageId);

        if (await repository.HasChildAsync(command.MessageId, ct))
            throw new OutboxMessageAlreadyResubmittedException(command.MessageId);

        var userId = requestContext.UserId
            ?? throw new InvalidOperationException("User context is not available for resubmission authorship.");
        var email = requestContext.UserEmail
            ?? throw new InvalidOperationException("User email is not available in context.");

        var replay = message.CreateResubmissionAttempt(userId, email);
        await repository.AddRetryAsync(replay, ct);

        return new ResubmitDeadLetterOutput(replay.Id);
    }
}
