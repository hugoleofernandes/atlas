using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Invitations.Handlers.Queries.ListInvitations;

public sealed class ListInvitationsQueryHandler : IListInvitationsQueryHandler
{
    private readonly IListInvitationsReader _reader;
    private readonly IRequestContext _context;

    public ListInvitationsQueryHandler(IListInvitationsReader reader, IRequestContext context)
    {
        _reader = reader;
        _context = context;
    }

    public Task<IReadOnlyList<InvitationDto>> ExecuteAsync(ListInvitationsQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.ListAsync(tenantId, query.IsActive, ct);
    }
}
