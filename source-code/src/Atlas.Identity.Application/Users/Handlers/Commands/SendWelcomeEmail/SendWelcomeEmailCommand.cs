namespace Atlas.Identity.Application.Users.Handlers.Commands.SendWelcomeEmail;

public sealed record SendWelcomeEmailCommand(
    Guid   TenantId,
    Guid   UserId,
    string Email
);
