using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Outbox.Application.Commands.ResubmitDeadLetter;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Modules;
using Atlas.Staff.Contracts.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Atlas.API.Endpoints.Outbox.ResubmitDeadLetter;

/// <summary>
/// Orchestrates module-owned outbox resubmission handlers for frontend convenience.
/// The module remains the owner of the command handler; this endpoint only routes by ModuleId.
/// </summary>
public sealed class ResubmitDeadLetterEndpoint(
    IIdentityResubmitDeadLetterCommandHandler identityHandler,
    IStaffResubmitDeadLetterCommandHandler staffHandler,
    IHandlerInvoker invoker,
    IAuthorizationService authorizationService)
    : AtlasEndpoint<ResubmitDeadLetterRequest, ResubmitDeadLetterResponse>
{
    public override void Configure()
    {
        Post("bff/v1/outbox/dead-letters/{Id}/resubmit");
        Description(d => d.Produces<ResubmitDeadLetterResponse>(200));
    }

    public override async Task HandleAsync(ResubmitDeadLetterRequest req, CancellationToken ct)
    {
        var target = ResolveTarget(req.ModuleId);
        if (target is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var authorization = await authorizationService.AuthorizeAsync(
            User,
            policyName: $"permission:{target.Permission}"
        );

        if (!authorization.Succeeded)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var result = await target.ExecuteAsync(new ResubmitDeadLetterCommand(req.Id), ct);
        await UpdatedFromResultAsync(
            result,
            output => ResubmitDeadLetterResponse.From(output, target.Module.Id, target.Module.Name),
            ct
        );
    }

    private ResubmitTarget? ResolveTarget(Guid moduleId)
    {
        if (moduleId == AtlasModules.Identity.Id)
        {
            return new ResubmitTarget(
                AtlasModules.Identity,
                IdentityModulePermissions.Outbox.Resubmit,
                (command, token) => invoker.InvokeAsync(identityHandler, command, token)
            );
        }

        if (moduleId == AtlasModules.Staff.Id)
        {
            return new ResubmitTarget(
                AtlasModules.Staff,
                StaffModulePermissions.Outbox.Resubmit,
                (command, token) => invoker.InvokeAsync(staffHandler, command, token)
            );
        }

        return null;
    }

    private sealed record ResubmitTarget(
        AtlasModule Module,
        string Permission,
        Func<ResubmitDeadLetterCommand, CancellationToken, Task<Result<ResubmitDeadLetterOutput>>> ExecuteAsync
    );
}
