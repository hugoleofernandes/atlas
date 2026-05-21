namespace Atlas.SharedKernel.Application;
public interface IRequestContext
{
    bool IsAuthenticated { get; }
    Guid? TenantId { get; }
    string? TenantName { get; }
    Guid? UserId { get; }
    string? UserEmail { get; }
    string? CorrelationId { get; }
}