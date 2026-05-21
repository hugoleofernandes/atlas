namespace Atlas.SharedKernel.Application;

public interface IRequestContextSetter
{
    void Set(Guid tenantId, string name, Guid userId, string? userEmail);
    void SetCorrelationId(string correlationId);
}