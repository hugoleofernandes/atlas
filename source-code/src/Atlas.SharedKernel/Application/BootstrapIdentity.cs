namespace Atlas.SharedKernel.Application;

/// <summary>
/// Deterministic, well-known identifiers for the bootstrap aggregates created at startup.
///
/// The root tenant and root user are created once with these fixed IDs,
/// so every subsequent run can look them up by ID — no fuzzy queries needed.
/// All bootstrap seed data is audited under RootUser.
/// </summary>
public static class BootstrapIdentity
{
    public static class RootTenant
    {
        public static readonly Guid   Id   = new("00000000-0000-0000-0000-000000000010");
        public static readonly string Name = "tenant01";
    }

    public static class RootUser
    {
        public static readonly Guid   Id    = new("00000000-0000-0000-0000-000000000020");
        public static readonly string Email = "root@atlas.local";
    }
}
