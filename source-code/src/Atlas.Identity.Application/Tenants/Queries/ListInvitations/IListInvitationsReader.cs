using Atlas.Identity.Application.Tenants.Queries.Dtos;

namespace Atlas.Identity.Application.Tenants.Queries.ListInvitations;

public interface IListInvitationsReader
{
    Task<IReadOnlyList<InvitationDto>> ListAsync(Guid tenantId, bool isActive, CancellationToken ct);
}
