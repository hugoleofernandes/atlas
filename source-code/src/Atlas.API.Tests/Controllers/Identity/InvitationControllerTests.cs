using System.Net;
using System.Net.Http.Json;
using Atlas.API.Controllers.Identity;
using Atlas.API.Tests.Infrastructure;
using Atlas.Identity.Application.Tenants.Commands.InviteUser;
using Atlas.Identity.Application.Tenants.Workflows.InviteUser;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;

namespace Atlas.API.Tests.Controllers.Identity;

public sealed class InvitationControllerTests(AtlasApiFactory factory)
    : IClassFixture<AtlasApiFactory>
{
    private readonly IInviteUserWorkflow _workflow = factory.InviteUserWorkflow;

    [Fact]
    public async Task Invite_AuthenticatedWithPermission_Returns201WithBody()
    {
        var roleId       = Guid.NewGuid();
        var invitationId = Guid.NewGuid();
        var expiresAt    = DateTime.UtcNow.AddDays(7);

        _workflow.ExecuteAsync(Arg.Any<InviteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new InviteUserOutput(invitationId, "new@acme.com", roleId, "Member", expiresAt)));

        var client = factory.CreateAuthenticatedClient(PermissionCatalog.Tenant.InviteUser);

        var response = await client.PostAsJsonAsync("/tenants/invitations", new
        {
            email  = "new@acme.com",
            roleId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<InviteUserResponse>();
        body!.InvitationId.Should().Be(invitationId);
        body.Email.Should().Be("new@acme.com");
        body.RoleId.Should().Be(roleId);
        body.RoleName.Should().Be("Member");
    }

    [Fact]
    public async Task Invite_WithoutAuthentication_Returns401()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.PostAsJsonAsync("/tenants/invitations", new
        {
            email  = "any@acme.com",
            roleId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Invite_AuthenticatedWithoutPermission_Returns403()
    {
        // authenticated but holds no permissions
        var client = factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/tenants/invitations", new
        {
            email  = "any@acme.com",
            roleId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Invite_WithNonGuidRoleId_Returns400()
    {
        var client = factory.CreateAuthenticatedClient(PermissionCatalog.Tenant.InviteUser);

        // "not-a-guid" cannot be deserialized to Guid — model binding rejects it
        var response = await client.PostAsJsonAsync("/tenants/invitations", new
        {
            email  = "any@acme.com",
            roleId = "not-a-guid"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invite_WhenEmailAlreadyInvited_Returns409()
    {
        var error = new ErrorDefinition(
            "invitation.already_exists",
            "This email has already been invited.",
            ErrorCategory.Conflict);

        _workflow.ExecuteAsync(Arg.Any<InviteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<InviteUserOutput>(error));

        var client = factory.CreateAuthenticatedClient(PermissionCatalog.Tenant.InviteUser);

        var response = await client.PostAsJsonAsync("/tenants/invitations", new
        {
            email  = "existing@acme.com",
            roleId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Invite_WhenRoleNotFound_Returns404()
    {
        var error = new ErrorDefinition(
            "role.not_found",
            "The requested role does not exist.",
            ErrorCategory.NotFound);

        _workflow.ExecuteAsync(Arg.Any<InviteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<InviteUserOutput>(error));

        var client = factory.CreateAuthenticatedClient(PermissionCatalog.Tenant.InviteUser);

        var response = await client.PostAsJsonAsync("/tenants/invitations", new
        {
            email  = "new@acme.com",
            roleId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Invite_TenantIsResolvedFromSession_NotFromRequestBody()
    {
        InviteUserCommand? capturedCommand = null;

        _workflow.ExecuteAsync(Arg.Do<InviteUserCommand>(c => capturedCommand = c), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(new InviteUserOutput(
                Guid.NewGuid(), "new@acme.com", Guid.NewGuid(), "Member", DateTime.UtcNow.AddDays(7))));

        // The client carries AtlasApiFactory.TestTenantName in its claims
        var client = factory.CreateAuthenticatedClient(PermissionCatalog.Tenant.InviteUser);

        await client.PostAsJsonAsync("/tenants/invitations", new
        {
            email  = "new@acme.com",
            roleId = Guid.NewGuid()
        });

        // The command does NOT include a tenant parameter — tenant comes from IRequestContext
        capturedCommand.Should().NotBeNull();
        capturedCommand!.Email.Should().Be("new@acme.com");
    }
}
