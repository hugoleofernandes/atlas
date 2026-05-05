using Atlas.Identity.Domain.Tenants;
using FluentAssertions;

namespace Atlas.Identity.Tests.Tenants.Domain;

public class WhenEmailAlreadyInvitedTests
{
    [Fact]
    public void InviteUser_ShouldThrow_WhenEmailAlreadyInvited()
    {
        var tenant = new Tenant("test");

        tenant.InviteUser("user@test.com", "admin");

        var act = () => tenant.InviteUser("user@test.com", "admin");

        act.Should().Throw<InvalidOperationException>();
    }
}