namespace Atlas.Identity.Contracts.Commands.SendWelcomeEmail;

public sealed record SendWelcomeEmailCommand(Guid TenantId, Guid UserId, string Email);
