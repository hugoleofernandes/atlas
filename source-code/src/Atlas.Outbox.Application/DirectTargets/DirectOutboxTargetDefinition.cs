namespace Atlas.Outbox.Application.DirectTargets;

/// <summary>
/// Explicit direct-mode mapping for one outbox event target.
/// Step 2 uses Name/Order for resolution; Step 3 uses HandlerContractType
/// to select the local executor behind the target.
/// </summary>
public sealed record DirectOutboxTargetDefinition(
    Type EventType,
    string Name,
    Type HandlerContractType,
    int Order = 0);
