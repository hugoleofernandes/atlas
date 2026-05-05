using Atlas.Identity.Application.Abstractions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Infrastructure.Persistence;

public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _db;
    private readonly IDomainEventDispatcher _dispatcher;

    public IdentityUnitOfWork(
        IdentityDbContext db,
        IDomainEventDispatcher dispatcher)
    {
        _db = db;
        _dispatcher = dispatcher;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        // 1. Coletar eventos antes de salvar
        var events = _db.GetDomainEvents().ToList();

        // 2. Publicar eventos
        await _dispatcher.DispatchAsync(events, ct);

        // 3. Persistir estado (incluindo efeitos dos handlers)
        await _db.SaveChangesAsync(ct);

        // 4. Limpar eventos
        _db.ClearDomainEvents();
    }
}