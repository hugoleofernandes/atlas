using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;

namespace Atlas.Party.BffApi.Endpoints.Shared;

/// <summary>
/// Maps the HTTP AddressRequest shape to the domain AddressInput, building the PostalAddress
/// value object. Reused by Register/Update endpoints for both Person and Organization.
/// </summary>
internal static class AddressRequestMapper
{
    public static IReadOnlyList<AddressInput> ToAddressInputs(IReadOnlyList<AddressRequest>? addresses)
    {
        if (addresses is null || addresses.Count == 0)
            return [];

        return addresses
            .Select(a => new AddressInput(
                a.Type,
                PostalAddress.Create(a.Street, a.Number, a.Complement, a.District, a.City, a.State, a.ZipCode, a.Country ?? "BR"),
                a.IsPrimary
            ))
            .ToList();
    }
}

