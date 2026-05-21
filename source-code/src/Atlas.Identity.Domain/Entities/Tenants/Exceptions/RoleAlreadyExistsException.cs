using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Exceptions;

public sealed class RoleAlreadyExistsException : DomainException
{
    public const string ErrorCode = "role.already_exists";

    public RoleAlreadyExistsException(string roleName)
        : base(ErrorCode, ErrorCategory.Conflict,
            $"A role named '{roleName}' already exists in this tenant.")
    {
    }
}
