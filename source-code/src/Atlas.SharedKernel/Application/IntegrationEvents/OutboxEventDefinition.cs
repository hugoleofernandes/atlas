using System.Text.Json;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

public sealed record OutboxEventDefinition(
    string Name,
    Type Type,
    string Module
);