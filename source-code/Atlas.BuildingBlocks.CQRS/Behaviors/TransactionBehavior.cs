using Atlas.BuildingBlocks.CQRS.Abstractions;
using Atlas.SharedKernel.Application;
using MediatR;

namespace Atlas.BuildingBlocks.CQRS.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IUnitOfWork> _unitOfWorks;

    public TransactionBehavior(IEnumerable<IUnitOfWork> unitOfWorks)
    {
        _unitOfWorks = unitOfWorks;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next();

        // só comita se for command
        if (request is ICommand<TResponse>)
        {
            foreach (var uow in _unitOfWorks)
                await uow.SaveChangesAsync(ct);
        }

        return response;
    }
}