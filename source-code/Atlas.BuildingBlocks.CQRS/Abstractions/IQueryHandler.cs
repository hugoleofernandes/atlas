using MediatR;

namespace Atlas.BuildingBlocks.CQRS.Abstractions;

public interface IQueryHandler<in TQuery, TResult>
    : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}