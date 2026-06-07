using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants._Roles._Permissions;

/// <summary>
/// Represents a single permission assigned to a Role.
/// </summary>
public sealed class Permission : ValueObject
{
    public string Code { get; }
    public string Group { get; }
    public bool IsManager { get; }

    private Permission(string code, string group, bool isManager)
    {
        Code = code;
        Group = group;
        IsManager = isManager;
    }

    public static Permission Of(string code, string group, bool isManager) => new(code, group, isManager);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
        yield return Group;
        yield return IsManager;
    }

    public override string ToString() => Code;
}
