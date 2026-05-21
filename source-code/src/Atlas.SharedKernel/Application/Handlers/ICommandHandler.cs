using Atlas.SharedKernel.Application;

namespace Atlas.SharedKernel.Application.Handlers;

/// <summary>
/// Contract for command handlers.
/// Specific command handler interfaces should extend this (e.g. ICreateRoleCommandHandler).
///
/// The handler owns its <see cref="IUnitOfWork"/> and exposes it so that
/// <see cref="Atlas.BuildingBlocks.Infrastructure.Workflows.IHandlerInvoker"/> can call
/// SaveChangesAsync as an explicit pipeline step after execution succeeds.
/// </summary>
public interface ICommandHandler<TCommand, TOutput> : IHandler<TCommand, TOutput>
{
    /// <summary>
    /// The unit of work scoped to this handler's domain.
    /// The invoker calls <see cref="IUnitOfWork.SaveChangesAsync"/> after a successful execution.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
}
