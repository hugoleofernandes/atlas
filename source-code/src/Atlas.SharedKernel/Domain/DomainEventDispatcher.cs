//using MediatR;
//using Atlas.SharedKernel.Domain;

//public sealed class DomainEventDispatcher : IDomainEventDispatcher
//{
//    private readonly IMediator _mediator;

//    public DomainEventDispatcher(IMediator mediator)
//    {
//        _mediator = mediator;
//    }

//    public async Task DispatchAsync(IEnumerable<DomainEvent> events, CancellationToken ct)
//    {
//        foreach (var domainEvent in events)
//            await _mediator.Publish(domainEvent, ct);
//    }
//}

//public interface IDomainEventDispatcher
//{
//    Task DispatchAsync(IEnumerable<DomainEvent> events, CancellationToken ct);
//}

//todo: deletar

