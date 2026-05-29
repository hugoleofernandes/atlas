using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.ResolveTenantAccess;

public interface IResolveTenantAccessCommandHandler : ICommandHandler<ResolveTenantAccessCommand, ResolveTenantAccessOutput>
{
}
