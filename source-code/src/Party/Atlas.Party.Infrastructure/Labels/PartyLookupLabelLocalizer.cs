using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Resources.Lookups;
using Microsoft.Extensions.Localization;

namespace Atlas.Party.Infrastructure.Labels;

public sealed class PartyLookupLabelLocalizer(IStringLocalizer<PartyLookupLabels> localizer) : IPartyLookupLabelLocalizer
{
    public string GetAddressTypeName(string code) => Get($"party.lookup.address-type.{code}", code);

    public string GetClassificationTypeName(string code) => Get($"party.lookup.classification-type.{code}", code);

    public string GetContactTypeName(string code) => Get($"party.lookup.contact-type.{code}", code);

    public string GetGenderName(string code) => Get($"party.lookup.gender.{code}", code);

    private string Get(string key, string fallback)
    {
        var value = localizer[key];
        return value.ResourceNotFound ? fallback : value.Value;
    }
}
