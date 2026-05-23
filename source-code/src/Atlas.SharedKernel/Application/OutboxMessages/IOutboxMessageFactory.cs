namespace Atlas.SharedKernel.Application.OutboxMessages;

public interface IOutboxMessageFactory
{
    /// <summary>
    /// Creates an <see cref="OutboxMessage"/> using the ambient <see cref="IRequestContext"/>
    /// for tenant, user, and correlation values.
    /// Use this in normal request-scoped handlers where the context is already populated.
    /// </summary>
    OutboxMessage Create<T>(T payload);

    /// <summary>
    /// Creates an <see cref="OutboxMessage"/> with explicitly supplied identity values,
    /// bypassing <see cref="IRequestContext"/>.
    ///
    /// Use this when the request context is not yet populated — for example in
    /// <c>ResolveTenantAccessCommandHandler</c>, which runs inside
    /// <c>UserBootstrapMiddleware</c> before tenant/user claims are written to the cookie.
    /// In that case the domain event itself already carries the correct
    /// <paramref name="tenantId"/>, <paramref name="userId"/>, and <paramref name="userEmail"/>.
    /// </summary>
    OutboxMessage Create<T>(T payload, Guid tenantId, Guid userId, string? userEmail);
}
