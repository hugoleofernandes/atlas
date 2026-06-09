namespace Atlas.Identity.Application.Commands.SendInvitationEmail;

public sealed record SendInvitationEmailCommand(Guid TenantId, string Email);
