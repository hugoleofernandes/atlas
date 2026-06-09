using Atlas.BuildingBlocks.Audit.Labels;
using System.Text.Json;

namespace Atlas.BuildingBlocks.Audit.Queries;

public sealed record AuditEntryResponse(
    Guid     Id,
    Guid     EntityTypeId,
    string   EntityTypeLabel,
    string   Action,
    string   ActionLabel,
    string?  EntityId,
    string?  UserId,
    string?  UserEmail,
    DateTime OccurredAtUtc,
    string   ChangesJson,
    IReadOnlyList<AuditChangeResponse> Changes)
{
    public static IReadOnlyList<AuditEntryResponse> FromList(
        IReadOnlyList<AuditEntryDto> result,
        AuditLabelLocalizer localizer)
    {
        var response = result.Select(x => From(x, localizer)).ToList();
        return response;
    }

    private static AuditEntryResponse From(AuditEntryDto entry, AuditLabelLocalizer localizer)
        => new(
            entry.Id,
            entry.EntityTypeId,
            localizer.LocalizeEntityType(entry.EntityTypeId),
            entry.Action,
            localizer.LocalizeAction(entry.Action),
            entry.EntityId,
            entry.UserId,
            entry.UserEmail,
            entry.OccurredAtUtc,
            entry.ChangesJson,
            AuditChangeResponse.FromJson(entry.ChangesJson));
}

public sealed record AuditChangeResponse(
    string  Field,
    object? Old,
    object? New)
{
    public static IReadOnlyList<AuditChangeResponse> FromJson(string changesJson)
    {
        if (string.IsNullOrWhiteSpace(changesJson))
            return [];

        try
        {
            using var document = JsonDocument.Parse(changesJson);
            if (document.RootElement.ValueKind is not JsonValueKind.Object)
                return [];

            var changes = new List<AuditChangeResponse>();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind is not JsonValueKind.Object)
                    continue;

                property.Value.TryGetProperty("Old", out var oldValue);
                property.Value.TryGetProperty("New", out var newValue);

                FlattenChange(changes, property.Name, oldValue, newValue);
            }

            return changes;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static void FlattenChange(
        List<AuditChangeResponse> changes,
        string                    field,
        JsonElement               oldValue,
        JsonElement               newValue)
    {
        oldValue = UnwrapSimpleValueObject(oldValue);
        newValue = UnwrapSimpleValueObject(newValue);

        if (ShouldFlatten(oldValue, newValue))
        {
            foreach (var childField in GetChildFields(oldValue).Union(GetChildFields(newValue)))
            {
                var childOld = TryGetProperty(oldValue, childField, out var oldChild)
                    ? oldChild
                    : default;

                var childNew = TryGetProperty(newValue, childField, out var newChild)
                    ? newChild
                    : default;

                FlattenChange(changes, $"{field}.{childField}", childOld, childNew);
            }

            return;
        }

        changes.Add(new AuditChangeResponse(
            field,
            Normalize(oldValue),
            Normalize(newValue)));
    }

    private static bool ShouldFlatten(JsonElement oldValue, JsonElement newValue)
        => oldValue.ValueKind is JsonValueKind.Object
           || newValue.ValueKind is JsonValueKind.Object;

    private static IEnumerable<string> GetChildFields(JsonElement element)
        => element.ValueKind is JsonValueKind.Object
            ? element.EnumerateObject().Select(property => property.Name)
            : [];

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement property)
    {
        if (element.ValueKind is JsonValueKind.Object)
            return element.TryGetProperty(propertyName, out property);

        property = default;
        return false;
    }

    private static JsonElement UnwrapSimpleValueObject(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Object
            && element.TryGetProperty("Value", out var value)
            && element.EnumerateObject().Count() == 1)
        {
            return value;
        }

        return element;
    }

    private static object? Normalize(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return null;

        if (element.ValueKind is JsonValueKind.String)
            return element.GetString();

        if (element.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return element.GetBoolean();

        if (element.ValueKind is JsonValueKind.Number)
            return element.TryGetInt64(out var integer)
                ? integer
                : element.GetDecimal();

        if (element.ValueKind is JsonValueKind.Array)
            return element.EnumerateArray()
                .Select(Normalize)
                .ToList();

        if (element.ValueKind is JsonValueKind.Object)
        {
            return element.EnumerateObject()
                .ToDictionary(
                    property => property.Name,
                    property => Normalize(property.Value));
        }

        return element.GetRawText();
    }
}
