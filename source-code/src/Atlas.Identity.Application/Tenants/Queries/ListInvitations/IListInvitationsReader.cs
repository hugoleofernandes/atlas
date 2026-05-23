using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.Queries.ListInvitations;

public interface IListInvitationsReader
{
    Task<PagedResult<InvitationDto>> ListAsync(Guid tenantId, int page, int pageSize, CancellationToken ct);
}
