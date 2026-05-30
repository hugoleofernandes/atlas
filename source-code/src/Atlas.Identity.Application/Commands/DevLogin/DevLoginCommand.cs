namespace Atlas.Identity.Application.Commands.DevLogin;

public sealed record DevLoginCommand(string TenantName, string Email);
