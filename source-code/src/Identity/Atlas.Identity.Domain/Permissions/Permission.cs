using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Permissions;

/// <summary>
/// Canonical permission entry in the Identity catalog.
/// Synced from module declarations at bootstrap; never hard-deleted.
/// system.root is the sole permission with IsRoot = true and no module.
/// </summary>
public sealed class Permission : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid? ModuleId { get; private set; }
    public string? ModuleName { get; private set; }
    public string Code { get; private set; } = default!;
    public string Group { get; private set; } = default!;
    public bool IsManager { get; private set; }
    public bool IsRoot { get; private set; }
    public bool IsActive { get; private set; }

    private Permission() { }

    public static Permission Create(
        Guid id,
        Guid moduleId,
        string moduleName,
        string code,
        string group,
        bool isManager)
    {
        return new Permission
        {
            Id = id,
            ModuleId = moduleId,
            ModuleName = moduleName,
            Code = code,
            Group = group,
            IsManager = isManager,
            IsRoot = false,
            IsActive = true,
        };
    }

    public static Permission CreateRoot(Guid id)
    {
        return new Permission
        {
            Id = id,
            ModuleId = null,
            ModuleName = null,
            Code = SystemPermissions.Root,
            Group = "system",
            IsManager = false,
            IsRoot = true,
            IsActive = true,
        };
    }

    /// <summary>
    /// Updates metadata from a module declaration during catalog sync.
    /// </summary>
    public void Sync(Guid moduleId, string moduleName, string group, bool isManager)
    {
        ModuleId = moduleId;
        ModuleName = moduleName;
        Group = group;
        IsManager = isManager;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
