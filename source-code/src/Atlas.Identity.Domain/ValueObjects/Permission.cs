namespace Atlas.Identity.Domain.ValueObjects;

/// <summary>
/// Represents a single permission assigned to a Role.
/// The Code is the stable contract between domain code and authorization checks.
/// </summary>
public sealed record Permission
{
    public string Code { get; }

    private Permission(string code) => Code = code;

    public static Permission Of(string code) => new(code);

    public override string ToString() => Code;
}
