using Atlas.BuildingBlocks.Infrastructure.Validation;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Atlas.BuildingBlocks.Infrastructure.Workflows;

public abstract class WorkflowBase<TCommand, TOutput>
{
    private readonly IValidator<TCommand> _validator;
    private readonly ILogger _logger;

    protected WorkflowBase(IValidator<TCommand> validator, ILoggerFactory loggerFactory)
    {
        _validator = validator;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task<Result<TOutput>> ExecuteAsync(TCommand cmd, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return Result.Fail<TOutput>(validation.ToErrorDefinition());

        var workflowName = GetType().Name;
        var sw = Stopwatch.StartNew();

        using (_logger.BeginScope(new Dictionary<string, object?> { ["WorkflowName"] = workflowName }))
        {
            _logger.LogInformation("Workflow {Workflow} started", workflowName);

            try
            {
                var output = await HandleAsync(cmd, ct);

                _logger.LogInformation("Workflow {Workflow} succeeded in {ElapsedMs}ms",
                    workflowName, sw.ElapsedMilliseconds);

                return Result.Ok(output);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning("Workflow {Workflow} failed in {ElapsedMs}ms — {ErrorCode}",
                    workflowName, sw.ElapsedMilliseconds, ex.ErrorCode);

                return Result.Fail<TOutput>(new ErrorDefinition(ex.ErrorCode, ex.Message, ex.Category));
            }
        }
    }

    protected abstract Task<TOutput> HandleAsync(TCommand cmd, CancellationToken ct);
}
