namespace Atlas.SharedKernel.Domain;

/// <summary>
/// Opt-out marker interface for audit logging.
/// Entities implementing this interface are excluded from automatic audit tracking.
/// By default, all entities are audited — implement this only when auditing
/// is explicitly unwanted (e.g. audit logs themselves, outbox messages).
/// </summary>
public interface INotAuditable { }
