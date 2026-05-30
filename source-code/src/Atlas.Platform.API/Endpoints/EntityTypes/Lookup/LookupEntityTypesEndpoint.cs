using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Atlas.Platform.Domain.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Platform.API.Endpoints.EntityTypes.Lookup;

public sealed class LookupEntityTypesEndpoint(
    ILookupEntityTypesQueryHandler handler,
    IHandlerInvoker                invoker
) : AtlasEndpoint<EmptyRequest, IReadOnlyList<EntityTypeLookupDto>>
{
    public override void Configure()
    {
        Get("entity-types/lookup");
        Policies($"permission:{PlatformModulePermissions.Audit.Read}");
        Description(d => d.Produces<IReadOnlyList<EntityTypeLookupDto>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new LookupEntityTypesQuery(), ct);
        await OkFromResultAsync(result, ct);
    }
}
