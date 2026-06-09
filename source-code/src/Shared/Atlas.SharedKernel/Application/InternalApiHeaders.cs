namespace Atlas.SharedKernel.Application;

public static class InternalApiHeaders
{
    public const string ApiKey = "X-Internal-Api-Key";
    public const string IdempotencyKey = "X-Idempotency-Key";
    public const string OutboxMessageId = "X-Outbox-Message-Id";
    public const string OutboxSubscription = "X-Outbox-Subscription";
    public const string CorrelationId = "X-Correlation-Id";
    public const string TenantId = "X-Tenant-Id";
    public const string UserId = "X-User-Id";
    public const string UserEmail = "X-User-Email";
    public const string TraceParent = "traceparent";
}
