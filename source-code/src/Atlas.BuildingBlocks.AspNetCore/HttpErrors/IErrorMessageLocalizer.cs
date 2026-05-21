using Atlas.SharedKernel.Application.Errors;

namespace Atlas.BuildingBlocks.AspNetCore.HttpErrors;

public interface IErrorMessageLocalizer
{
    string Localize(ErrorDefinition error);
}



