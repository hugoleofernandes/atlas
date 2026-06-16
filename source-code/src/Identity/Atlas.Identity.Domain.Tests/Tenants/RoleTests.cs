using Atlas.Identity.Domain.Roles;
using Atlas.Identity.Domain.Roles.Events;
using Atlas.Identity.Domain.Roles.Exceptions;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants;

public class RoleTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldCreateRole_WithCorrectData()
    {
        var perm = PermissionFixtures.Any();
        var role = Role.Create(TenantId, "support", [perm]);

        role.TenantId.Should().Be(TenantId);
        role.Name.Should().Be("support");
        role.IsSystem.Should().BeFalse();
        role.IsActive.Should().BeTrue();
        role.Permissions.Should().ContainSingle(p => p.PermissionId == perm.PermissionId);
    }

    [Fact]
    public void Create_ShouldEmitRoleCreatedEvent()
    {
        var role = Role.Create(TenantId, "support", []);

        var evt = role.DomainEvents.OfType<RoleCreatedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameIsTooShort()
    {
        var act = () => Role.Create(TenantId, "ab", []);

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameIsTooLong()
    {
        var act = () => Role.Create(TenantId, "toolongname", []);

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void Create_ShouldUseProvidedId_WhenIdIsSupplied()
    {
        var fixedId = Guid.NewGuid();

        var role = Role.Create(TenantId, "support", [], id: fixedId);

        role.Id.Should().Be(fixedId);
    }

    [Fact]
    public void Create_ShouldMarkAsSystem_WhenIsSystemIsTrue()
    {
        var role = Role.Create(TenantId, "admin", PermissionFixtures.Many(3), isSystem: true);

        role.IsSystem.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldNormalizeDuplicatePermissions_ByPermissionId()
    {
        var id = Guid.NewGuid();
        var duplicated = new[] { RolePermission.Of(id), RolePermission.Of(id) };

        var role = Role.Create(TenantId, "custom", duplicated);

        role.Permissions.Should().HaveCount(1);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var role = Role.Create(TenantId, "support", []);

        role.Deactivate();

        role.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldEmitRoleDeactivatedEvent()
    {
        var role = Role.Create(TenantId, "support", []);
        role.ClearDomainEvents();

        role.Deactivate();

        var evt = role.DomainEvents.OfType<RoleDeactivatedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void Delete_ShouldEmitRoleDeletedEvent()
    {
        var role = Role.Create(TenantId, "support", []);
        role.ClearDomainEvents();

        role.Delete();

        var evt = role.DomainEvents.OfType<RoleDeletedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void Rename_ShouldUpdateName_WhenNameIsValid()
    {
        var role = Role.Create(TenantId, "custom", []);

        role.Rename("renamed");

        role.Name.Should().Be("renamed");
    }

    [Fact]
    public void Rename_ShouldThrow_WhenNameIsTooShort()
    {
        var role = Role.Create(TenantId, "custom", []);

        var act = () => role.Rename("ab");

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void Rename_ShouldThrow_WhenNameIsTooLong()
    {
        var role = Role.Create(TenantId, "custom", []);

        var act = () => role.Rename("toolongname");

        act.Should().Throw<InvalidRoleNameException>();
    }

    [Fact]
    public void UpdatePermissions_ShouldReplacePermissions_WhenRoleIsCustom()
    {
        var perm1 = PermissionFixtures.Any();
        var perm2 = PermissionFixtures.Any();
        var role = Role.Create(TenantId, "custom", [perm1]);
        role.ClearDomainEvents();

        role.UpdatePermissions([perm1, perm2]);

        role.Permissions.Should().HaveCount(2);
        role.Permissions.Select(p => p.PermissionId).Should().Contain([perm1.PermissionId, perm2.PermissionId]);
    }

    [Fact]
    public void UpdatePermissions_ShouldEmitRoleUpdatedEvent()
    {
        var role = Role.Create(TenantId, "custom", []);
        role.ClearDomainEvents();

        role.UpdatePermissions([PermissionFixtures.Any()]);

        var evt = role.DomainEvents.OfType<RoleUpdatedDomainEvent>().Single();
        evt.TenantId.Should().Be(TenantId);
        evt.RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void UpdatePermissions_ShouldThrow_WhenRoleIsSystem()
    {
        var role = Role.Create(TenantId, "admin", PermissionFixtures.Many(2), isSystem: true);

        var act = () => role.UpdatePermissions([PermissionFixtures.Any()]);

        act.Should().Throw<SystemRoleCannotBeModifiedException>();
    }
}
