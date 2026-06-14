using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Outbox.Application.Queries.ListOutboxMessages;
using Atlas.Platform.Application.Queries.Tenants.GetTenantsByIds;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.Staff.Contracts.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Atlas.API.Endpoints.Outbox.ListMessages;

/// <summary>
/// Aggregates module-owned outbox message queries for frontend convenience.
/// Each module remains the owner of its query handler; this endpoint only composes the results.
/// </summary>
public sealed class ListOutboxMessagesEndpoint(
    IIdentityListOutboxMessagesQueryHandler identityHandler,
    IStaffListOutboxMessagesQueryHandler staffHandler,
    IGetTenantsByIdsQueryHandler getTenantsByIdsHandler,
    IHandlerInvoker invoker,
    IAuthorizationService authorizationService
) : AtlasEndpoint<ListOutboxMessagesRequest, IReadOnlyList<ListOutboxMessagesResponse>>
{
    public override void Configure()
    {
        Get("bff/v1/outbox/messages");
        Description(d => d.Produces<IReadOnlyList<ListOutboxMessagesResponse>>());
    }

    public override async Task HandleAsync(ListOutboxMessagesRequest req, CancellationToken ct)
    {
        var canReadIdentity = await CanReadAsync(IdentityModulePermissions.Outbox.Read);
        var canReadStaff = await CanReadAsync(StaffModulePermissions.Outbox.Read);

        if (!canReadIdentity && !canReadStaff)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var query = new ListOutboxMessagesQuery(req.From, req.To);
        var tasks = new List<Task<Result<IReadOnlyList<OutboxMessageRow>>>>();

        if (canReadIdentity)
            tasks.Add(invoker.InvokeAsync(identityHandler, query, ct));

        if (canReadStaff)
            tasks.Add(invoker.InvokeAsync(staffHandler, query, ct));

        var results = await Task.WhenAll(tasks);

        var failure = results.FirstOrDefault(result => !result.IsSuccess);
        if (failure is not null && !failure.IsSuccess)
        {
            await SendErrorAsync(failure.ErrorDefinition!);
            return;
        }

        var rows = results
            .SelectMany(result => result.Value!)
            .OrderByDescending(row => row.OccurredOn)
            .ToList()
            .AsReadOnly();

        var tenantIds = rows
            .Select(row => row.TenantId)
            .Distinct()
            .ToArray();

        var tenantsResult = await invoker.InvokeAsync(
            getTenantsByIdsHandler,
            new GetTenantsByIdsQuery(tenantIds),
            ct
        );

        if (!tenantsResult.IsSuccess)
        {
            await SendErrorAsync(tenantsResult.ErrorDefinition!);
            return;
        }

        var tenantsById = tenantsResult.Value!
            .ToDictionary(tenant => tenant.TenantId, tenant => tenant.TenantName);

        var response = rows
            .Select(row =>
                ListOutboxMessagesResponse.From(
                    row,
                    tenantsById.GetValueOrDefault(row.TenantId)
                ))
            .ToList()
            .AsReadOnly();

        await Send.OkAsync(response, ct);
    }

    private async Task<bool> CanReadAsync(string permission)
    {
        var authorization = await authorizationService.AuthorizeAsync(
            User,
            policyName: $"permission:{permission}"
        );

        return authorization.Succeeded;
    }
}
