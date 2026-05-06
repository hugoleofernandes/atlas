using Atlas.SharedKernel.Domain;
using Atlas.Identity.Domain.ValueObjects.Exceptions;

namespace Atlas.Identity.Domain.ValueObjects;

/// <summary>
/// Represents a user role within a tenant.
///
/// Invariants:
/// - Role must be one of the allowed domain roles.
/// - Role must not be null or empty.
/// - Role is normalized to lowercase.
///
/// Purpose:
/// - Ensures consistent handling of roles across the domain.
/// - Prevents primitive obsession and invalid role assignments.
/// </summary>
public sealed class Role : ValueObject
{
    public string Value { get; }

    private static readonly HashSet<string> AllowedRoles =
    [
        "admin",
        "member",
        "owner"
    ];

    private Role(string value)
    {
        Value = value;
    }

    public static Role Create(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new InvalidRoleException(role);

        var normalized = role.Trim().ToLowerInvariant();

        if (!AllowedRoles.Contains(normalized))
            throw new InvalidRoleException(role);

        return new Role(normalized);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Role role) => role.Value;
}
