using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.Entities.Tenants.Users;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public sealed class UserTests
{
    private static readonly Guid DefaultRoleId = Guid.NewGuid();

    internal static class UserBuilder
    {
        public static User Create(
            Guid? tenantId = null,
            string externalId = "oid-123",
            string email = "user@test.com",
            Guid? roleId = null)
        {
            return new User(
                tenantId ?? Guid.NewGuid(),
                ExternalId.Create(externalId),
                Email.Create(email),
                roleId ?? DefaultRoleId
            );
        }
    }

    // ------------------------------------------------------------
    // 1. Constructor Behavior
    // ------------------------------------------------------------

    [Fact]
    public void Constructor_ShouldCreateActiveUser_WithValidData()
    {
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var user = new User(
            tenantId,
            ExternalId.Create("oid-123"),
            Email.Create("user@test.com"),
            roleId
        );

        user.TenantId.Should().Be(tenantId);
        user.ExternalId.Value.Should().Be("oid-123");
        user.Email.Value.Should().Be("user@test.com");
        user.RoleId.Should().Be(roleId);
        user.IsActive.Should().BeTrue();
    }

    // ------------------------------------------------------------
    // 2. ChangeRole Behavior
    // ------------------------------------------------------------

    [Fact]
    public void ChangeRole_ShouldUpdateTenantRoleId_WhenNewRoleProvided()
    {
        var originalRoleId = Guid.NewGuid();
        var newRoleId = Guid.NewGuid();
        var user = UserBuilder.Create(roleId: originalRoleId);

        user.ChangeRole(newRoleId);

        user.RoleId.Should().Be(newRoleId);
    }

    [Fact]
    public void ChangeRole_ShouldBeIdempotent_WhenSameRoleIdProvided()
    {
        var roleId = Guid.NewGuid();
        var user = UserBuilder.Create(roleId: roleId);

        user.ChangeRole(roleId);

        user.RoleId.Should().Be(roleId);
    }

    // ------------------------------------------------------------
    // 3. Deactivate Behavior
    // ------------------------------------------------------------

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse_WhenUserIsActive()
    {
        var user = UserBuilder.Create();

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldRemainInactive_WhenCalledMultipleTimes()
    {
        var user = UserBuilder.Create();

        user.Deactivate();
        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }
}
