using Atlas.Outbox.Application.Queries.ListOutboxMessages;
using Atlas.SharedKernel.Modules;
using FluentAssertions;

namespace Atlas.Outbox.Tests.Queries;

public sealed class OutboxMessageRowTests
{
    [Fact]
    public void NormalizedName_RemovesIntegrationEventSuffix()
    {
        var row = CreateRow("UserInvitedIntegrationEvent");

        row.NormalizedName.Should().Be("UserInvited");
    }

    [Fact]
    public void NormalizedName_RemovesDomainEventSuffix()
    {
        var row = CreateRow("UserInvitedDomainEvent");

        row.NormalizedName.Should().Be("UserInvited");
    }

    [Fact]
    public void NormalizedName_KeepsOriginalName_WhenNoKnownSuffixExists()
    {
        var row = CreateRow("UserInvited");

        row.NormalizedName.Should().Be("UserInvited");
    }

    private static OutboxMessageRow CreateRow(string name, string status = "Pending", bool hasReplayChild = false) =>
        new(
            Id: Guid.NewGuid(),
            ModuleId: AtlasModules.Identity.Id,
            ModuleName: AtlasModules.Identity.Name,
            IdempotencyKey: Guid.NewGuid(),
            ParentOutboxMessageId: null,
            AttemptNumber: 1,
            Name: name,
            OccurredOn: DateTime.UtcNow,
            Status: status,
            Origin: "Automatic",
            Error: null,
            ProcessedOn: null,
            FailedAt: null,
            DeadLetteredOn: null,
            TenantId: Guid.NewGuid(),
            UserEmail: null,
            CorrelationId: Guid.NewGuid().ToString(),
            ResubmittedByEmail: null,
            HasReplayChild: hasReplayChild,
            ExecutionCount: 0,
            Executions: []
        );
}
