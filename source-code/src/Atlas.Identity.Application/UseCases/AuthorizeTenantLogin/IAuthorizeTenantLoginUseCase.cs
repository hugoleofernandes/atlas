namespace Atlas.Identity.Application.UseCases.AuthorizeTenantLogin;

public interface IAuthorizeTenantLoginUseCase
{
    Task<AuthorizeTenantLoginResult> ExecuteAsync(
        AuthorizeTenantLoginCommand command,
        CancellationToken ct);
}