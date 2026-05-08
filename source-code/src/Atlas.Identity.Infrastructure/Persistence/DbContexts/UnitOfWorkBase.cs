//using Atlas.BuildingBlocks.Persistence;
//using Atlas.Identity.Application.Abstractions;
//using Atlas.SharedKernel.Application;

//namespace Atlas.Identity.Infrastructure.Persistence;

//public abstract class UnitOfWorkBase
//{
//    private readonly AuditableDbContext _db;

//    public UnitOfWorkBase(AuditableDbContext db,
//        IRequestContext requestContext,
//        IIntegrationEventMapper integrationEventMapper)
//    {
//        _db = db;
//        _requestContext = requestContext;
//        _mapper = integrationEventMapper;
//    }

//    public async Task SaveChangesAsync(CancellationToken ct)
//    {
//        //var events = _db.GetDomainEvents();

//        //var messages = events
//        //    .Select(e => _mapper.Map(e, _requestContext))
//        //    .Where(x => x is not null)
//        //    .Cast<OutboxMessage>()
//        //    .ToList();

//        //_db.OutboxMessages.AddRange(messages);

//        _db.ClearDomainEvents();

//        await _db.SaveChangesAsync(ct);
//    }

//    public async Task AddOutboxMessage(OutboxMessage message) {
//        _db.OutboxMessages.Add(message);
//    }
//}

//todo:deletar