    using MediatR;

    namespace Atlas.BuildingBlocks.CQRS.Abstractions;

    public interface ICommand<out TResult> : IRequest<TResult>
    {
    }