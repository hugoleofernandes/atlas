//using Atlas.Identity.Domain.Tenants;
//using FluentAssertions;

//namespace Atlas.Identity.Tests.Tenants.Domain;

//public class WhenUserNotInvitedTests
//{
//    [Fact]
//    public void GetOrBindMembership_ShouldThrow_WhenUserNotInvited()
//    {
//        var tenant = new Tenant("test");

//        var act = () => tenant.BindUserToMembershipByEmail(Guid.NewGuid(), "nope@test.com");

//        act.Should().Throw<InvalidOperationException>()
//            .WithMessage("User not invited to this tenant.");
//    }
//}