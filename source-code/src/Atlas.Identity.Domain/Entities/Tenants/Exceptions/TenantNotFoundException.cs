namespace Atlas.Identity.Domain.Exceptions;

/// <summary>
/// Represents the business rule that a tenant must exist
/// before any access resolution or user-related operation
/// can be performed within the Identity domain.
///
/// This exception is thrown when the application layer
/// attempts to load a tenant by name and no matching
/// aggregate is found.
/// </summary>
public sealed class TenantNotFoundException : Exception
{
    public TenantNotFoundException(string tenantName)
        : base($"Tenant '{tenantName}' was not found.")
    {
    }
}
