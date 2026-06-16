using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared.Exceptions;

public sealed class TenantContextNotResolvedException : DomainException
{
    public const string ErrorCode = "tenant.context_not_resolved";

    public TenantContextNotResolvedException()
        : base(ErrorCode, ErrorCategory.Unauthorized, "Tenant context could not be resolved for this request.") { }
}
