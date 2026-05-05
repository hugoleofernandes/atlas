//using Atlas.Identity.Domain.Tenants;
//using FluentAssertions;

//namespace Atlas.Identity.Tests.Tenants.Domain;

//public class WhenInvitedTests
//{
//    [Fact]
//    public void GetOrBindMembership_ShouldBindUser_WhenInvited()
//    {
//        // Arrange
//        var tenant = new Tenant("test");
//        var email = "user@test.com";
//        var userId = Guid.NewGuid();

//        tenant.InviteUser(email, "admin");

//        // Act
//        var membership = tenant.BindUserToMembershipByEmail(userId, email);

//        // Assert
//        membership.UserId.Should().Be(userId);
//        membership.Email.Should().Be(email.ToLowerInvariant());
//    }
//}