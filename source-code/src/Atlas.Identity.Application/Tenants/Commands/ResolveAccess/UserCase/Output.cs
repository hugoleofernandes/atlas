using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.Commands.ResolveAccess.UserCase;

public sealed class Output : OutputBase
{
    public Result Result { get; }


    public Output(Tenant tenant, Result result) 
        : base(tenant)
    {
        Result = result;
    }
}