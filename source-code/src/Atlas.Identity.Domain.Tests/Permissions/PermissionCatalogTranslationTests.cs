using Atlas.Identity.Domain.Tenants.Roles.Permissions;
using Atlas.Staff.Domain.Permissions;

using FluentAssertions;
using System.Xml.Linq;

namespace Atlas.Identity.Tests.Permissions;

/// <summary>
/// Contract test: every permission code registered across all modules must have
/// a non-empty label in both the English and Portuguese PermissionLabels.resx files.
///
/// Why: the compiler guarantees the code exists, but cannot guarantee the translation.
/// This test closes that gap — a missing translation fails the build immediately.
/// </summary>
public sealed class PermissionCatalogTranslationTests
{
    private static readonly string ResxDirectory = FindResxDirectory();

    /// <summary>
    /// The full permission policy built from all registered modules — same as runtime.
    /// </summary>
    private static readonly PermissionPolicyService Policy = new(
    [
        new IdentityPermissions(),
        new StaffPermissions(),
    ]);

    [Fact]
    public void AllPermissions_ShouldHaveLabel_InEnglishResx()
    {
        var labels = LoadLabels("PermissionLabels.resx");
        var missing = Policy.All
            .Where(code => !labels.ContainsKey(code) || string.IsNullOrWhiteSpace(labels[code]))
            .ToList();

        missing.Should().BeEmpty(
            because: $"every permission must have an English label. Missing: {string.Join(", ", missing)}");
    }

    [Fact]
    public void AllPermissions_ShouldHaveLabel_InPortugueseResx()
    {
        var labels = LoadLabels("PermissionLabels.pt.resx");
        var missing = Policy.All
            .Where(code => !labels.ContainsKey(code) || string.IsNullOrWhiteSpace(labels[code]))
            .ToList();

        missing.Should().BeEmpty(
            because: $"every permission must have a Portuguese label. Missing: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EnglishResx_ShouldNotHaveOrphanedKeys_NotInCatalog()
    {
        var labels = LoadLabels("PermissionLabels.resx");
        var orphaned = labels.Keys
            .Where(key => !Policy.All.Contains(key))
            .ToList();

        orphaned.Should().BeEmpty(
            because: $"PermissionLabels.resx contains keys not in any module catalog (dead translations): {string.Join(", ", orphaned)}");
    }

    [Fact]
    public void PortugueseResx_ShouldNotHaveOrphanedKeys_NotInCatalog()
    {
        var labels = LoadLabels("PermissionLabels.pt.resx");
        var orphaned = labels.Keys
            .Where(key => !Policy.All.Contains(key))
            .ToList();

        orphaned.Should().BeEmpty(
            because: $"PermissionLabels.pt.resx contains keys not in any module catalog (dead translations): {string.Join(", ", orphaned)}");
    }

    // -------------------------------------------------------

    private static Dictionary<string, string> LoadLabels(string fileName)
    {
        var path = Path.Combine(ResxDirectory, fileName);
        File.Exists(path).Should().BeTrue(because: $"{fileName} must exist at {path}");

        return XDocument.Load(path)
            .Descendants("data")
            .Where(e => e.Attribute("name") is not null)
            .ToDictionary(
                e => e.Attribute("name")!.Value,
                e => e.Element("value")?.Value ?? string.Empty
            );
    }

    /// <summary>
    /// Walks up from the test output directory until it finds the solution root
    /// (identified by the presence of Atlas.slnx), then navigates to the resx folder.
    /// Works both locally and in CI as long as source files are present.
    /// </summary>
    private static string FindResxDirectory()
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null)
        {
            if (Directory.GetFiles(dir, "*.slnx").Length > 0)
                return Path.Combine(dir, "src", "Atlas.API", "Resources");

            dir = Path.GetDirectoryName(dir);
        }

        throw new DirectoryNotFoundException(
            "Could not find solution root (no *.slnx file found walking up from test output).");
    }
}
