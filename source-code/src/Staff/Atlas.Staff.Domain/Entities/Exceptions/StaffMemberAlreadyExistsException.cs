using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Staff.Domain.Entities.Exceptions;

public sealed class StaffMemberAlreadyExistsException : DomainException
{
    public const string ErrorCode = "staff_member.already_exists";

    public StaffMemberAlreadyExistsException(Guid partyId)
        : base(ErrorCode, ErrorCategory.Conflict, $"A staff member for party '{partyId}' already exists in this tenant.") { }
}
