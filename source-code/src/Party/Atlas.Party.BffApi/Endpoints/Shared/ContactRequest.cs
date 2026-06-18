using Atlas.Party.Domain.Shared;

namespace Atlas.Party.BffApi.Endpoints.Shared;

public sealed record ContactRequest(ContactType Type, string Value, bool IsPrimary);
