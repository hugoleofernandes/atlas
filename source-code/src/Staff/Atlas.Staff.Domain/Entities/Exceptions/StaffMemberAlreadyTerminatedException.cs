using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.Staff.Domain.Entities.Exceptions;

public sealed class StaffMemberAlreadyTerminatedException : DomainException
{
    public const string ErrorCode = "staff_member.already_terminated";

    public StaffMemberAlreadyTerminatedException(Guid staffMemberId)
        : base(ErrorCode, ErrorCategory.Business, $"Staff member '{staffMemberId}' is already terminated.") { }
}
