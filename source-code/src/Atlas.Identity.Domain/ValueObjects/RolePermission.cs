namespace Atlas.Identity.Domain.ValueObjects;

/// <summary>
/// Represents a single permission assigned to a TenantRole.
/// The Code is the stable contract between domain code and authorization checks.
/// </summary>
public sealed record RolePermission
{
    public string Code { get; }

    private RolePermission(string code) => Code = code;

    public static RolePermission Of(string code) => new(code);

    public override string ToString() => Code;
}
