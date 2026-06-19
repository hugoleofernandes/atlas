using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Queries.Permissions.ListPermissions;

namespace Atlas.Identity.BffApi.Endpoints.Permissions.ListPermissions;

public sealed record PermissionItemResponse(
    Guid ModuleId,
    string ModuleName,
    string Code,
    string Group,
    bool IsActive,
    string Label)
{
    public static IReadOnlyList<PermissionItemResponse> FromList(
        IReadOnlyList<PermissionItemDto> result,
        PermissionLabelLocalizer localizer
    )
    {
        var response = result.Select(x => ToResponse(x, localizer)).ToList();
        return response;
    }

    private static PermissionItemResponse ToResponse(PermissionItemDto dto, PermissionLabelLocalizer localizer) =>
        new(dto.ModuleId, dto.ModuleName, dto.Code, dto.Group, dto.IsActive, localizer.Localize(dto.Code));
}
