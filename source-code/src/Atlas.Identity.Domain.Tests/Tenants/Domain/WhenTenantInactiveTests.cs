//using Atlas.Identity.Domain.Tenants;
//using FluentAssertions;

//namespace Atlas.Identity.Tests.Tenants.Domain;

//public class WhenTenantInactiveTests
//{
//    [Fact]
//    public void InviteUser_ShouldThrow_WhenTenantInactive()
//    {
//        // Arrange
//        var tenant = new Tenant("test");
//        tenant.Deactivate();

//        // Act
//        var act = () => tenant.InviteUser("user@test.com", "admin");

//        // Assert
//        act.Should().Throw<InvalidOperationException>()
//            .WithMessage("Tenant is inactive.");
//    }
//}