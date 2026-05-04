using MediatR;

namespace Atlas.BuildingBlocks.CQRS.Abstractions;

public interface IQuery<out TResult> : IRequest<TResult>
{
}