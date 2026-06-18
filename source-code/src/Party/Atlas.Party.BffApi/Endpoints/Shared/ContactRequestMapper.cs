using Atlas.Party.Domain.Parties;

namespace Atlas.Party.BffApi.Endpoints.Shared;

internal static class ContactRequestMapper
{
    public static IReadOnlyList<ContactInput> ToContactInputs(IReadOnlyList<ContactRequest>? contacts)
    {
        if (contacts is null || contacts.Count == 0)
            return [];

        return contacts
            .Select(c => new ContactInput(c.Type, c.Value, c.IsPrimary))
            .ToList();
    }
}
