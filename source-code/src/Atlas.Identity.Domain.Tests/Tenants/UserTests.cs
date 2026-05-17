using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.ValueObjects;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public sealed class UserTests
{
    internal static class UserBuilder
    {
        public static User Create(
            Guid? tenantId = null,
            string externalId = "oid-123",
            string email = "user@test.com",
            string role = "admin")
        {
            return new User(
                tenantId ?? Guid.NewGuid(),
                ExternalId.Create(externalId),
                Email.Create(email),
                Role.Create(role)
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

        var user = new User(
            tenantId,
            ExternalId.Create("oid-123"),
            Email.Create("user@test.com"),
            Role.Create("admin")
        );

        user.TenantId.Should().Be(tenantId);
        user.ExternalId.Value.Should().Be("oid-123");
        user.Email.Value.Should().Be("user@test.com");
        user.Role.Value.Should().Be("admin");
        user.IsActive.Should().BeTrue();
    }

    // ------------------------------------------------------------
    // 2. ChangeRole Behavior
    // ------------------------------------------------------------

    [Fact]
    public void ChangeRole_ShouldUpdateRole_WhenValidRoleProvided()
    {
        var user = UserBuilder.Create(role: "admin");

        user.ChangeRole(Role.Create("member"));

        user.Role.Value.Should().Be("member");
    }

    [Fact]
    public void ChangeRole_ShouldBeIdempotent_WhenSameRoleIsProvided()
    {
        var user = UserBuilder.Create(role: "admin");

        user.ChangeRole(Role.Create("admin"));

        user.Role.Value.Should().Be("admin");
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
