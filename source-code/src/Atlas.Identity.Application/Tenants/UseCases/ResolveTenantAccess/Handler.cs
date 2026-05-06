using Atlas.BuildingBlocks.CQRS.Abstractions;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Errors;
using Atlas.Identity.Application.Tenants.Abstractions;
using Atlas.Identity.Domain.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Application;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

/// <summary>
/// Orchestrates the process of resolving user access within a tenant.
///
/// Responsibilities:
/// - Load the Tenant aggregate
/// - Trigger the ResolveAccess domain behavior
/// - Persist changes through the Unit of Work
/// - Return a DTO reflecting the domain result
///
/// This use case does NOT:
/// - Enforce domain invariants
/// - Apply business rules
/// - Perform domain validation
/// - Mutate domain state directly
/// </summary>
public sealed class Handler
	: ICommandHandler<Command, Result<ResultDto>>
{
	private readonly ITenantRepository _tenantRepository;
	private readonly IIdentityUnitOfWork _unitOfWork;

	public Handler(
		ITenantRepository tenantRepository,
		IIdentityUnitOfWork unitOfWork)
	{
		_tenantRepository = tenantRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<ResultDto>> Handle(
		Command command,
		CancellationToken ct)
	{
		var tenant = await _tenantRepository
			.GetByNameWithUsersAndInvitationsAsync(
				command.TenantName.ToLowerInvariant(), ct);

		if (tenant is null)
			return Result<ResultDto>.Failure(TenantErrors.NotFound);

		var user = tenant.ResolveAccess(
			ExternalId.Create(command.ExternalOid),
			Email.Create(command.Email));

		//await _unitOfWork.SaveChangesAsync(ct);

		return Result<ResultDto>.Ok(
			new ResultDto(
				tenant.Id,
				tenant.Name,
				user.Id,
				user.Role.Value));
	}
}