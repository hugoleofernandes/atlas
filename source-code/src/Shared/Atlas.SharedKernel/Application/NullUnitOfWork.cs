namespace Atlas.SharedKernel.Application;

/// <summary>
/// Null Object Pattern implementation of <see cref="IUnitOfWork"/>.
///
/// Use for command handlers that perform no database writes
/// (e.g. sending e-mail, calling an external API, publishing to a queue).
///
/// <see cref="SaveChangesAsync"/> is a deliberate no-op so that
/// <c>PersistDbDecorator</c> can call it unconditionally without side-effects.
/// </summary>
public sealed class NullUnitOfWork : IUnitOfWork
{
    public static readonly NullUnitOfWork Instance = new();

    private NullUnitOfWork() { }

    /// <inheritdoc/>
    /// <remarks>No-op — this handler makes no database changes.</remarks>
    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
