using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Commands.ResolveTenantAccess;

public interface IResolveTenantAccessCommandHandler : ICommandHandler<ResolveTenantAccessCommand, ResolveTenantAccessOutput>
{
}
