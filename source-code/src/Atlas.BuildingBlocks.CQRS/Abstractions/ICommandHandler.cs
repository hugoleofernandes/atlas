using MediatR;

namespace Atlas.BuildingBlocks.CQRS.Abstractions;

public interface ICommandHandler<in TCommand, TResult>
    : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
}