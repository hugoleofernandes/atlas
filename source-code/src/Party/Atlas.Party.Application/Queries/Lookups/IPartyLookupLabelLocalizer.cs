namespace Atlas.Party.Application.Queries.Lookups;

public interface IPartyLookupLabelLocalizer
{
    string GetAddressTypeName(string code);

    string GetClassificationTypeName(string code);

    string GetContactTypeName(string code);

    string GetGenderName(string code);
}
