namespace Atlas.SharedKernel.Application.Errors;

public interface IErrorMessageLocalizer
{
    string Localize(ErrorDefinition error);
}



