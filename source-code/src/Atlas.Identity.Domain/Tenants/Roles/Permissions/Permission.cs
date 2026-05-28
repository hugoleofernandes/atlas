using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Roles.Permissions;

/// <summary>
/// Represents a single permission assigned to a Role.
/// The Code is the stable contract between domain code and authorization checks.
/// </summary>
public sealed class Permission : ValueObject
{
    public string Code { get; }

    private Permission(string code) => Code = code;

    public static Permission Of(string code) => new(code);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => Code;
}
