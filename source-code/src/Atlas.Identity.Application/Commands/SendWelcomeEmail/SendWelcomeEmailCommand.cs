namespace Atlas.Identity.Application.Commands.SendWelcomeEmail;

public sealed record SendWelcomeEmailCommand(
    Guid   TenantId,
    Guid   UserId,
    string Email
);
