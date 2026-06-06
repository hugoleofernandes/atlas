using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Outbox.Application.Commands.ProcessOutbox;

public interface IProcessOutboxCommandHandler : ICommandHandler<ProcessOutboxCommand, ProcessOutboxOutput> { }

/// <summary>Marker for the Identity module's outbox handler registration.</summary>
public interface IIdentityOutboxCommandHandler : IProcessOutboxCommandHandler { }

/// <summary>Marker for the Staff module's outbox handler registration.</summary>
public interface IStaffOutboxCommandHandler : IProcessOutboxCommandHandler { }
