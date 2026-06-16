using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Roles.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Commands.ActivateRole;

public sealed class ActivateRoleCommandHandler : IActivateRoleCommandHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRequestContext _requestContext;
    private readonly IIdentityUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public ActivateRoleCommandHandler(
        IRoleRepository roleRepository,
        IRequestContext requestContext,
        IIdentityUnitOfWork uow
    )
    {
        _roleRepository = roleRepository;
        _requestContext = requestContext;
        _uow = uow;
    }

    public async Task<ActivateRoleOutput> ExecuteAsync(ActivateRoleCommand cmd, CancellationToken ct)
    {
        _ = _requestContext.TenantId ?? throw new TenantContextNotResolvedException();

        var role =
            await _roleRepository.GetByIdWithPermissionsAsync(cmd.RoleId, ct)
            ?? throw new RoleNotFoundException(cmd.RoleId);

        if (role.IsSystem)
            throw new SystemRoleCannotBeModifiedException(role.Name);

        role.Activate();

        return new ActivateRoleOutput(role.Id, role.IsActive);
    }
}
