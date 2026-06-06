using System.Xml.Linq;
using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Contracts.Permissions;
using FluentAssertions;
using StaffContracts = Atlas.Staff.Contracts.Permissions;

namespace Atlas.Identity.Tests.Permissions;

/// <summary>
/// Contract test: every permission code registered across all modules must have
/// a non-empty label in both the English and Portuguese PermissionLabels.resx files.
///
/// Why: the compiler guarantees the code exists, but cannot guarantee the translation.
/// This test closes that gap â€” a missing translation fails the build immediately.
/// </summary>
public sealed class PermissionCatalogTranslationTests
{
    private static readonly string SharedResxDirectory = FindResxDirectory();

    /// <summary>
    /// The full permission policy built from all registered modules â€” same as runtime.
    /// </summary>
    private static readonly PermissionPolicyService Policy = new([
        new ModulePermissions(),
        new StaffContracts.ModulePermissions(),
        new Platform.Domain.ModulePermissions.ModulePermissions(),
    ]);

    [Fact]
    public void AllPermissions_ShouldHaveLabel_InEnglishResx()
    {
        var labels = LoadAllLabels("resx");
        var missing = Policy
            .All.Where(code => !labels.ContainsKey(code) || string.IsNullOrWhiteSpace(labels[code]))
            .ToList();

        missing
            .Should()
            .BeEmpty(because: $"every permission must have an English label. Missing: {string.Join(", ", missing)}");
    }

    [Fact]
    public void AllPermissions_ShouldHaveLabel_InPortugueseResx()
    {
        var labels = LoadAllLabels("pt.resx");
        var missing = Policy
            .All.Where(code => !labels.ContainsKey(code) || string.IsNullOrWhiteSpace(labels[code]))
            .ToList();

        missing
            .Should()
            .BeEmpty(because: $"every permission must have a Portuguese label. Missing: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EnglishResx_ShouldNotHaveOrphanedKeys_NotInCatalog()
    {
        var labels = LoadAllLabels("resx");
        var orphaned = labels.Keys.Where(key => !Policy.All.Contains(key)).ToList();

        orphaned
            .Should()
            .BeEmpty(
                because: $"Permission resx files contain keys not in any module catalog (dead translations): {string.Join(", ", orphaned)}"
            );
    }

    [Fact]
    public void PortugueseResx_ShouldNotHaveOrphanedKeys_NotInCatalog()
    {
        var labels = LoadAllLabels("pt.resx");
        var orphaned = labels.Keys.Where(key => !Policy.All.Contains(key)).ToList();

        orphaned
            .Should()
            .BeEmpty(
                because: $"Permission resx files contain keys not in any module catalog (dead translations): {string.Join(", ", orphaned)}"
            );
    }

    // -------------------------------------------------------

    /// <summary>Loads labels from the shared domain resource project for the given file extension.</summary>
    private static Dictionary<string, string> LoadAllLabels(string extension)
    {
        var path = Path.Combine(SharedResxDirectory, $"PermissionLabels.{extension}");
        File.Exists(path).Should().BeTrue(because: $"resource file must exist at {path}");

        return XDocument
            .Load(path)
            .Descendants("data")
            .Where(e => e.Attribute("name") is not null)
            .Select(e => (Key: e.Attribute("name")!.Value, Value: e.Element("value")?.Value ?? string.Empty))
            .ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Walks up from the test output directory until it finds the solution root
    /// (identified by the presence of Atlas.slnx), then navigates to the shared resource project.
    /// </summary>
    private static string FindResxDirectory()
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null)
        {
            if (Directory.GetFiles(dir, "*.slnx").Length > 0)
                return Path.Combine(dir, "src", "Atlas.SharedDomain.Resources", "Permissions");

            dir = Path.GetDirectoryName(dir);
        }

        throw new DirectoryNotFoundException(
            "Could not find solution root (no *.slnx file found walking up from test output)."
        );
    }
}
