namespace Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;

/// <summary>
/// Creates a StaffMember when a user accepts an invitation.
/// Email is carried here so the handler can derive a placeholder FirstName
/// (e.g. "hugo.silva@company.com" → "hugo.silva") until the user fills their profile.
/// </summary>
public sealed record CreateStaffMemberFromInvitationCommand(
    Guid   TenantId,
    Guid   UserId,
    string Email,
    string Role
);
