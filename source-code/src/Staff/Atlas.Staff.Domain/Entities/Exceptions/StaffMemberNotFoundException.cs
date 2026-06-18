using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Staff.Domain.Entities.Exceptions;

public sealed class StaffMemberNotFoundException : DomainException
{
    public const string ErrorCode = "staff_member.not_found";

    public StaffMemberNotFoundException(Guid staffMemberId)
        : base(ErrorCode, ErrorCategory.NotFound, $"Staff member '{staffMemberId}' not found.") { }
}
