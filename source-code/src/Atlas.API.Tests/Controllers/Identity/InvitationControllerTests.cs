using System.Net;
using System.Net.Http.Json;
using Atlas.API.Models.Invitations;
using Atlas.API.Tests.Infrastructure;
using Atlas.Identity.Application.Tenants.Commands.InviteUser;
using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Application.Tenants.Queries.ListInvitations;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Permissions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Atlas.API.Tests.Controllers.Identity;

public sealed class InvitationControllerTests(AtlasApiFactory factory)
    : IClassFixture<AtlasApiFactory>
{
    private readonly IInviteUserCommandHandler _handler = factory.InviteUserHandler;
    private readonly IListInvitationsQueryHandler _listHandler = factory.ListInvitationsQueryHandler;

    [Fact]
    public async Task List_AuthenticatedWithPermission_Returns200WithInvitations()
    {
        var invitationId = Guid.NewGuid();
        var roleId       = Guid.NewGuid();
        var createdAt    = DateTime.UtcNow;
        IReadOnlyList<InvitationDto> invitations =
        [
            new InvitationDto(
                invitationId,
                "new@acme.com",
                roleId,
                "Member",
                DateTime.UtcNow.AddDays(7),
                false,
                true,
                createdAt,
                null,
                null,
                null,
                null,
                null)
        ];

        _listHandler.ExecuteAsync(
                Arg.Is<ListInvitationsQuery>(query => query.IsActive),
                Arg.Any<CancellationToken>())
            .Returns(invitations);

        var client = factory.CreateAuthenticatedClient(PermissionCatalog.Tenant.InviteUser);

        var response = await client.GetAsync("/tenants/invitations?isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<InvitationDto>>();
        body.Should().NotBeNull();
        body.Should().ContainSingle();
        body!.Single().InvitationId.Should().Be(invitationId);
        body.Single().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task List_WithoutAuthentication_Returns401()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/tenants/invitations");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task List_AuthenticatedWithoutPermission_Returns403()
    {
        var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/tenants/invitations");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Invite_AuthenticatedWithPermission_Returns201WithBody()
    {
        var roleId       = Guid.NewGuid();
        var invitationId = Guid.NewGuid();
        var expiresAt    = DateTime.UtcNow.AddDays(7);

        _handler.ExecuteAsync(Arg.Any<InviteUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(new InviteUserOutput(invitationId, "new@acme.com", roleId, "Member", expiresAt));

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
        _handler.ExecuteAsync(Arg.Any<InviteUserCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new DuplicateInvitationException("existing@acme.com"));

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
        _handler.ExecuteAsync(Arg.Any<InviteUserCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RoleNotFoundException(Guid.NewGuid()));

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

        _handler.ExecuteAsync(Arg.Do<InviteUserCommand>(c => capturedCommand = c), Arg.Any<CancellationToken>())
            .Returns(new InviteUserOutput(
                Guid.NewGuid(), "new@acme.com", Guid.NewGuid(), "Member", DateTime.UtcNow.AddDays(7)));

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
