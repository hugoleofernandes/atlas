namespace Atlas.SharedKernel.Domain;

/// <summary>
/// Opt-in interface for aggregate roots that participate in the audit trail.
///
/// Implementing this interface causes AuditTrailService to record changes using
/// EntityTypeId — a deterministic GUID defined in Atlas.SharedDomain.
///
/// Aggregates that do NOT implement this interface are silently skipped by the
/// audit trail, keeping the opt-in model clean and explicit.
/// </summary>
public interface IAuditableAggregate
{
    Guid EntityTypeId { get; }
}
