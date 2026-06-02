using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Application.Queries.Permissions.ListPermissions;

namespace Atlas.Identity.BffApi.Endpoints.Permissions.ListPermissions;

public sealed record PermissionItemResponse(Guid ModuleId, string ModuleName, string Code, string Label)
{
    public static PermissionItemResponse From(PermissionItemDto dto, PermissionLabelLocalizer localizer) =>
        new(dto.ModuleId, dto.ModuleName, dto.Code, localizer.Localize(dto.Code));
}
