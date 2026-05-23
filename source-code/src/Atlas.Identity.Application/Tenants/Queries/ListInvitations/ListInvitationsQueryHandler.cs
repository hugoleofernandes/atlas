using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.Queries.ListInvitations;

public sealed class ListInvitationsQueryHandler : IListInvitationsQueryHandler
{
    private readonly IListInvitationsReader _reader;
    private readonly IRequestContext _context;

    public ListInvitationsQueryHandler(IListInvitationsReader reader, IRequestContext context)
    {
        _reader = reader;
        _context = context;
    }

    public Task<PagedResult<InvitationDto>> ExecuteAsync(ListInvitationsQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.ListAsync(tenantId, query.Page, query.PageSize, ct);
    }
}
