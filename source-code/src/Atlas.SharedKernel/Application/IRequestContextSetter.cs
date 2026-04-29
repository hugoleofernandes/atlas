namespace Atlas.SharedKernel.Application;

public interface IRequestContextSetter
{
    void Set(Guid tenantId, string slug, Guid userId);
}