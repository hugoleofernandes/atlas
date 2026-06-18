using Atlas.Party.Domain.Parties;

namespace Atlas.Party.BffApi.Endpoints.Shared;

internal static class ClassificationRequestMapper
{
    public static IReadOnlyList<ClassificationInput> ToClassificationInputs(IReadOnlyList<ClassificationRequest>? classifications)
    {
        if (classifications is null || classifications.Count == 0)
            return [];

        return classifications
            .Select(c => new ClassificationInput(c.Type, c.Since ?? DateOnly.FromDateTime(DateTime.UtcNow), c.Until))
            .ToList();
    }
}
