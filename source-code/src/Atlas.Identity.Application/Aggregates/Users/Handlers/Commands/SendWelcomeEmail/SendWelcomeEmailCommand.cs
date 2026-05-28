namespace Atlas.Identity.Application.Aggregates.Users.Handlers.Commands.SendWelcomeEmail;

public sealed record SendWelcomeEmailCommand(
    Guid   TenantId,
    Guid   UserId,
    string Email
);
