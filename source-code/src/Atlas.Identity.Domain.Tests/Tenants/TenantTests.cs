using FluentAssertions;
using Atlas.Platform.Domain.Tenants;
using Atlas.Platform.Domain.Tenants.Events;
using Atlas.Platform.Domain.Tenants.Exceptions;

namespace Atlas.Identity.Tests.Tenants;

public class TenantTests
{
    // ============================================================
    // 1. CONSTRUCTOR
    // ============================================================

    [Fact]
    public void Tenant_ShouldThrow_WhenNameIsMissing()
    {
        var act = () => new Tenant("");

        act.Should().Throw<TenantNameRequiredException>();
    }

    [Fact]
    public void Tenant_ShouldNormalizeName_WhenCreated()
    {
        var tenant = new Tenant("MyTenant");

        tenant.Name.Should().Be("mytenant");
    }

    // ============================================================
    // 2. ENSURE ACTIVE
    // ============================================================

    [Fact]
    public void EnsureActive_ShouldThrow_WhenTenantIsInactive()
    {
        var tenant = new Tenant("test");
        tenant.Deactivate();

        var act = () => tenant.EnsureActive();

        act.Should().Throw<TenantInactiveException>();
    }

    [Fact]
    public void EnsureActive_ShouldNotThrow_WhenTenantIsActive()
    {
        var tenant = new Tenant("test");

        var act = () => tenant.EnsureActive();

        act.Should().NotThrow();
    }

    // ============================================================
    // 3. LIFECYCLE: DEACTIVATE
    // ============================================================

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse_WhenTenantIsActive()
    {
        var tenant = new Tenant("test");

        tenant.Deactivate();

        tenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldEmitTenantDeactivatedEvent_WhenTenantIsActive()
    {
        var tenant = new Tenant("test");

        tenant.Deactivate();

        tenant.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TenantDeactivatedDomainEvent>();
    }

    [Fact]
    public void Deactivate_ShouldEmitEventWithCorrectTenantId()
    {
        var tenant = new Tenant("test");

        tenant.Deactivate();

        var evt = tenant.DomainEvents.OfType<TenantDeactivatedDomainEvent>().Single();
        evt.TenantId.Should().Be(tenant.Id);
    }

    [Fact]
    public void Deactivate_ShouldDoNothing_WhenTenantIsAlreadyInactive()
    {
        var tenant = new Tenant("test");
        tenant.Deactivate();
        tenant.ClearDomainEvents();

        tenant.Deactivate();

        tenant.DomainEvents.Should().BeEmpty();
    }
}
