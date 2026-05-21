using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Domain;

/// <summary>
/// Base class for all domain exceptions.
/// Carries an ErrorCode (used as i18n key) and an ErrorCategory (used to determine HTTP status).
/// The GlobalExceptionMiddleware catches DomainException and uses these fields
/// to return a localized, correctly-typed HTTP response.
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }
    public ErrorCategory Category { get; }

    protected DomainException(string errorCode, ErrorCategory category, string technicalMessage)
        : base(technicalMessage)
    {
        ErrorCode = errorCode;
        Category = category;
    }
}
