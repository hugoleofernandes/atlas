using Atlas.Party.Domain.Shared;

namespace Atlas.Party.Domain.Parties;

public sealed record ClassificationInput(ClassificationType Type, DateOnly Since, DateOnly? Until);
