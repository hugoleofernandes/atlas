using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class SystemRoleCannotBeModifiedException : DomainException
{
    public const string ErrorCode = "role.system_role_immutable";

    public SystemRoleCannotBeModifiedException(string roleName)
        : base(ErrorCode, ErrorCategory.Business,
            $"System role '{roleName}' cannot be modified or deleted.")
    {
    }
}
