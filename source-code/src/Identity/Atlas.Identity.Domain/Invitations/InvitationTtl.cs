using Atlas.SharedKernel.Domain;
using Atlas.Identity.Domain.Invitations.Exceptions;

namespace Atlas.Identity.Domain.Invitations;

/// <summary>
/// Represents the time-to-live (TTL) of an invitation.
///
/// Invariants:
/// - TTL must be greater than zero.
/// - TTL must not exceed the maximum allowed duration.
///
/// Purpose:
/// - Ensures consistent handling of invitation expiration rules.
/// - Prevents invalid or extreme TTL values from entering the domain.
/// </summary>
public sealed class InvitationTtl : ValueObject
{
    public TimeSpan Value { get; }

    private static readonly TimeSpan MaxTtl = TimeSpan.FromDays(30);

    /// <summary>
    /// Default TTL defined by product: invitations expire after 7 days.
    /// </summary>
    public static readonly InvitationTtl Default = new(TimeSpan.FromDays(7));

    private InvitationTtl(TimeSpan value)
    {
        Value = value;
    }

    public static InvitationTtl Create(TimeSpan ttl)
    {
        if (ttl <= TimeSpan.Zero)
            throw new InvalidInvitationTtlException(ttl);

        if (ttl > MaxTtl)
            throw new InvalidInvitationTtlException(ttl);

        return new InvitationTtl(ttl);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
