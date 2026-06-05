namespace Atlas.Identity.Contracts.Commands.SendInvitationEmail;

public sealed record SendInvitationEmailCommand(Guid TenantId, string Email);
