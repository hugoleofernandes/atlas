namespace Atlas.SharedKernel.Domain.Permissions;

/// <summary>
/// Declares the relationship between a manage permission and its implied granular verbs.
/// Used by the frontend to render grouped checkboxes and by the authorization handler
/// to resolve a manage permission as satisfying any granular verb check.
/// </summary>
public sealed record PermissionGroup(string Manage, IReadOnlyList<string> Granular);
