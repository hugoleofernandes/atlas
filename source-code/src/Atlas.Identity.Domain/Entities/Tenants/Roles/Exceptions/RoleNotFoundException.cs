using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Roles.Exceptions;

public sealed class RoleNotFoundException : DomainException
{
    public const string ErrorCode = "role.not_found";

    public RoleNotFoundException(Guid roleId)
        : base(ErrorCode, ErrorCategory.NotFound,
            $"Role '{roleId}' not found in this tenant.")
    {
    }
}
