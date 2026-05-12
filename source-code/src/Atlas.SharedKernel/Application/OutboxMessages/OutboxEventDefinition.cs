using System.Text.Json;

namespace Atlas.SharedKernel.Application.OutboxMessages;

public sealed record OutboxEventDefinition(
    string Name,
    Type Type,
    string Module
);