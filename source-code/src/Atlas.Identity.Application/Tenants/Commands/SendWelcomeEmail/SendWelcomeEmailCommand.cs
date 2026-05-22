namespace Atlas.Identity.Application.Tenants.Commands.SendWelcomeEmail;

public sealed record SendWelcomeEmailCommand(
    Guid   TenantId,
    Guid   UserId,
    string Email
);
