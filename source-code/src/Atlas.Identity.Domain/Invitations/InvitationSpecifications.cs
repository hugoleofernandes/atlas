using System.Linq.Expressions;

namespace Atlas.Identity.Domain.Invitations;

public static class InvitationSpecifications
{
    public static Expression<Func<Invitation, bool>> Active(DateTime now) =>
        i => i.IsUsed == false && i.ExpiresAt >= now;

    public static Expression<Func<Invitation, bool>> Inactive(DateTime now) =>
        i => i.IsUsed == true || i.ExpiresAt < now;
}
