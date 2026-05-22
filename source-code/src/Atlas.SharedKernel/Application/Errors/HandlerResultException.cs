namespace Atlas.SharedKernel.Application.Errors;

/// <summary>
/// Thrown by integration event adapters when the command handler they delegate to
/// returns <c>Result.Fail</c>.
///
/// Carries the original <see cref="ErrorDefinition"/> so the invoker pipeline can
/// restore the structured failure — error code, category and message — rather than
/// discarding it inside a plain <see cref="System.InvalidOperationException"/>.
///
/// <see cref="Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators.DomainExceptionDecorator"/>
/// catches this and converts it back to <c>Result.Fail&lt;TOutput&gt;(ErrorDefinition)</c>,
/// which then flows through <c>LoggingDecorator</c> (logs error code) and
/// <c>TelemetryDecorator</c> (sets <c>atlas.error.code</c> and <c>atlas.error.category</c>
/// OTel tags) without any additional changes to those decorators.
///
/// Message format: <c>[{Code}] {FallbackMessage}</c> — identifiable in raw log output
/// even without structured fields.
/// </summary>
public sealed class HandlerResultException : Exception
{
    /// <summary>The original failure definition returned by the command handler.</summary>
    public ErrorDefinition ErrorDefinition { get; }

    public HandlerResultException(ErrorDefinition errorDefinition)
        : base($"[{errorDefinition.Code}] {errorDefinition.FallbackMessage}")
    {
        ErrorDefinition = errorDefinition;
    }
}
