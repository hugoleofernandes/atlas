namespace Atlas.Identity.Application.Commands.DevLogin;

public sealed record DevLoginCommand(Guid TenantId, string TenantName, string Email);
