using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;

/// <summary>
/// Delegates to the inner handler, then persists to the database on success.
///
/// Calls <see cref="IUnitOfWork.SaveChangesAsync"/>, which runs the full SavePipeline:
/// audit trail → tenant stamp → change stamp → integration events → DbContext.SaveChangesAsync.
///
/// NOT called if the inner handler throws — the exception propagates unchanged,
/// leaving the transaction uncommitted.
///
/// For workers that need to bypass the SavePipeline, inject DbContext directly
/// instead of going through HandlerInvoker.
/// </summary>
internal sealed class PersistDbDecorator<TInput, TOutput> : IHandler<TInput, TOutput>
{
    private readonly IHandler<TInput, TOutput> _inner;
    private readonly IUnitOfWork _unitOfWork;

    public PersistDbDecorator(IHandler<TInput, TOutput> inner, IUnitOfWork unitOfWork)
    {
        _inner     = inner;
        _unitOfWork = unitOfWork;
    }

    public async Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct)
    {
        var output = await _inner.ExecuteAsync(input, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return output;
    }
}
