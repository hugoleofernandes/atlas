using Atlas.SharedKernel.Application.Handlers;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.BuildingBlocks.Application.Invokers.Decorators;

/// <summary>
/// Resolves <see cref="IValidator{TInput}"/> from DI and validates the input
/// before delegating to the inner handler.
///
/// If no validator is registered for TInput, validation is skipped.
/// Throws <see cref="ValidationException"/> on invalid input — caught upstream
/// by <see cref="DomainExceptionDecorator{TInput,TOutput}"/>.
/// </summary>
internal sealed class ValidationDecorator<TInput, TOutput> : IHandler<TInput, TOutput>
{
    private readonly IHandler<TInput, TOutput> _inner;
    private readonly IServiceProvider _serviceProvider;

    public ValidationDecorator(IHandler<TInput, TOutput> inner, IServiceProvider serviceProvider)
    {
        _inner = inner;
        _serviceProvider = serviceProvider;
    }

    public async Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct)
    {
        var validator = _serviceProvider.GetService<IValidator<TInput>>();
        if (validator is not null)
        {
            var result = await validator.ValidateAsync(input, ct);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }

        return await _inner.ExecuteAsync(input, ct);
    }
}
