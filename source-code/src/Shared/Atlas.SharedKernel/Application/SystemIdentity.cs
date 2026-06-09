namespace Atlas.SharedKernel.Application;

/// <summary>
/// Represents the system identity used for operations that run outside
/// of a user request context (e.g. seeders, background jobs, migrations).
/// </summary>
public static class SystemIdentity
{
    public static readonly Guid   UserId = new("00000000-0000-0000-0000-000000000001");
    public static readonly string Email  = "system@atlas";
    public static readonly string Name   = "System";
}
