using Atlas.BuildingBlocks.CQRS.Abstractions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.UseCases.ResolveTenantAccess;

public sealed record Command(
    string TenantName,
    string ExternalOid,
    string Email
) : ICommand<Result<ResultDto>>;