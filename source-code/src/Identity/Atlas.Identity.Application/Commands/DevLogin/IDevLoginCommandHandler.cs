using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.DevLogin;

/// <summary>
/// Dev-only — neither a query (read intent) nor a command (write intent).
/// Uses IHandler directly so the invoker routes it through CommandHandlerInvoker
/// with NullUnitOfWork — observability and validation run, no DB write occurs.
/// </summary>
public interface IDevLoginCommandHandler : IHandler<DevLoginCommand, DevLoginOutput>;
