namespace Atlas.SharedKernel.Application.Logging;

/// <summary>
/// Opt-in interface for commands and integration events that want to provide
/// a PII-safe summary for production logs (Information level).
///
/// Without this interface → LoggingDecorator logs only the type name at Information level.
/// With this interface    → LoggingDecorator logs ToLogSummary() at Information level.
///
/// The full serialized payload is always available at Debug level regardless of
/// whether this interface is implemented — useful for investigation in dev/staging.
///
/// Implementation guide:
///   Include identifiers (TenantId, UserId, OrderId) — they help diagnose without exposing data.
///   Exclude PII (Email, Name, CPF, phone) — these must not appear in permanent production logs.
/// </summary>
public interface ILogSummary
{
    string ToLogSummary();
}
