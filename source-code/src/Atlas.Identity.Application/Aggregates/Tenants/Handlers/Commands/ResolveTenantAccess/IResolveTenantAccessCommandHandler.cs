using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Aggregates.Tenants.Handlers.Commands.ResolveTenantAccess;

public interface IResolveTenantAccessCommandHandler : ICommandHandler<ResolveTenantAccessCommand, ResolveTenantAccessOutput>
{
}
