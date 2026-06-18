using Atlas.Party.Domain.Shared;

namespace Atlas.Party.BffApi.Endpoints.Shared;

public sealed record ClassificationRequest(ClassificationType Type, DateOnly? Since, DateOnly? Until);
