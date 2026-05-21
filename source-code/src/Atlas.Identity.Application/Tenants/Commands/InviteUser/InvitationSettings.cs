namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public sealed class InvitationSettings
{
    public int TtlDays { get; init; } = 7;
}
