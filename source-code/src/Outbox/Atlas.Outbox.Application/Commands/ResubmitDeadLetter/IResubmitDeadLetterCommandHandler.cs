using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Outbox.Application.Commands.ResubmitDeadLetter;

public interface IResubmitDeadLetterCommandHandler
    : ICommandHandler<ResubmitDeadLetterCommand, ResubmitDeadLetterOutput> { }
