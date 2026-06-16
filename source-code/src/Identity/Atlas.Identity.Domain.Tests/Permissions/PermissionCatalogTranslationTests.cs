using System.Xml.Linq;
using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Party.Contracts.Permissions;
using Atlas.Staff.Contracts.Permissions;
using FluentAssertions;

namespace Atlas.Identity.Tests.Permissions;

/// <summary>
/// Contract test: every permission code declared by a module must have a non-empty
/// label in that module's PermissionLabels resx files (English and Portuguese).
///
/// Why: the compiler guarantees the code exists, but cannot guarantee the translation.
/// This test closes that gap — a missing translation fails the build immediately.
///
/// Platform is intentionally excluded: its 2 audit codes have hardcoded EN labels in
/// PlatformPermissionLabelProvider (see that class for the rationale).
/// </summary>
public sealed class PermissionCatalogTranslationTests
{
    private static readonly string SourceRoot = FindSourceRoot();

    private static readonly IReadOnlyList<(IModulePermissions Module, string ResxBasePath)> ModuleResxMap =
    [
        (
            new IdentityModulePermissions(),
            Path.Combine("Identity", "Atlas.Identity.Resources", "Permissions", "IdentityPermissionLabels")
        ),
        (
            new StaffModulePermissions(),
            Path.Combine("Staff", "Atlas.Staff.Resources", "Permissions", "StaffPermissionLabels")
        ),
        (
            new PartyModulePermissions(),
            Path.Combine("Party", "Atlas.Party.Resources", "Permissions", "PartyPermissionLabels")
        ),
    ];

    [Fact]
    public void AllPermissions_ShouldHaveLabel_InEnglishResx() => AssertAllCodesHaveLabels(extension: "resx");

    [Fact]
    public void AllPermissions_ShouldHaveLabel_InPortugueseResx() => AssertAllCodesHaveLabels(extension: "pt.resx");

    [Fact]
    public void EnglishResx_ShouldNotHaveOrphanedKeys_NotInCatalog() => AssertNoOrphanedKeys(extension: "resx");

    [Fact]
    public void PortugueseResx_ShouldNotHaveOrphanedKeys_NotInCatalog() => AssertNoOrphanedKeys(extension: "pt.resx");

    // -------------------------------------------------------

    private static void AssertAllCodesHaveLabels(string extension)
    {
        foreach (var (module, resxBasePath) in ModuleResxMap)
        {
            var labels = LoadLabels(resxBasePath, extension);
            var codes = module.Definitions.Select(d => d.Code).ToHashSet(StringComparer.Ordinal);

            var missing = codes
                .Where(code => !labels.ContainsKey(code) || string.IsNullOrWhiteSpace(labels[code]))
                .ToList();

            missing
                .Should()
                .BeEmpty(
                    because: $"every {module.ModuleName} permission must have a label in "
                        + $"{resxBasePath}.{extension}. Missing: {string.Join(", ", missing)}"
                );
        }
    }

    private static void AssertNoOrphanedKeys(string extension)
    {
        foreach (var (module, resxBasePath) in ModuleResxMap)
        {
            var labels = LoadLabels(resxBasePath, extension);
            var codes = module.Definitions.Select(d => d.Code).ToHashSet(StringComparer.Ordinal);

            var orphaned = labels.Keys.Where(key => !codes.Contains(key)).ToList();

            orphaned
                .Should()
                .BeEmpty(
                    because: $"{resxBasePath}.{extension} contains keys not in the {module.ModuleName} "
                        + $"catalog (dead translations): {string.Join(", ", orphaned)}"
                );
        }
    }

    private static Dictionary<string, string> LoadLabels(string resxBasePath, string extension)
    {
        var path = Path.Combine(SourceRoot, $"{resxBasePath}.{extension}");
        File.Exists(path).Should().BeTrue(because: $"resource file must exist at {path}");

        return XDocument
            .Load(path)
            .Descendants("data")
            .Where(e => e.Attribute("name") is not null)
            .Select(e => (Key: e.Attribute("name")!.Value, Value: e.Element("value")?.Value ?? string.Empty))
            .ToDictionary(x => x.Key, x => x.Value);
    }

    private static string FindSourceRoot()
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null)
        {
            if (Directory.GetFiles(dir, "*.slnx").Length > 0)
                return Path.Combine(dir, "src");

            dir = Path.GetDirectoryName(dir);
        }

        throw new DirectoryNotFoundException(
            "Could not find solution root (no *.slnx file found walking up from test output)."
        );
    }
}
