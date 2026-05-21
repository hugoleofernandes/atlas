namespace Atlas.SharedKernel.Domain;

/// <summary>
/// Opt-out marker interface for multi-tenant stamping.
/// Entities implementing this interface are excluded from automatic TenantId assignment.
/// By default, all entities receive a TenantId — implement this only when stamping
/// is explicitly unwanted (e.g. audit logs, which set TenantId directly via Initialize).
/// </summary>
public interface INotMultiTenant { }
