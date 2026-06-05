using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;

public interface IUpdateOutboxMessageStatusCommandHandler
    : ICommandHandler<UpdateOutboxMessageStatusCommand, UpdateOutboxMessageStatusOutput> { }
