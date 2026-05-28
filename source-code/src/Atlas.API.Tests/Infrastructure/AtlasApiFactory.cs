using Atlas.Identity.Application.Invitations.Handlers.Commands.InviteUser;
using Atlas.Identity.Application.Invitations.Handlers.Queries.ListInvitations;
using Atlas.SharedKernel.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Atlas.API.Tests.Infrastructure;

public sealed class AtlasApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid TestTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TestUserId   = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public const string TestTenantName = "acme";

    public IInviteUserCommandHandler InviteUserHandler { get; } = Substitute.For<IInviteUserCommandHandler>();
    public IListInvitationsQueryHandler ListInvitationsQueryHandler { get; } = Substitute.For<IListInvitationsQueryHandler>();

    public AtlasApiFactory()
    {
        // HandlerInvoker calls handler.UnitOfWork.SaveChangesAsync() after execution.
        // The mock must expose a no-op UnitOfWork so success-path tests don't NRE.
        InviteUserHandler.UnitOfWork.Returns(Substitute.For<IUnitOfWork>());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" environment skips the dev-only seeder/migration block in Program.cs
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Override default auth schemes so OIDC/Cookie handlers are never triggered
            services.PostConfigure<AuthenticationOptions>(opts =>
            {
                opts.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                opts.DefaultChallengeScheme    = TestAuthHandler.SchemeName;
                opts.DefaultForbidScheme       = TestAuthHandler.SchemeName;
                opts.DefaultSignInScheme       = TestAuthHandler.SchemeName;
            });

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            // Replace the real command handler with a controllable mock
            services.RemoveAll<IInviteUserCommandHandler>();
            services.AddScoped(_ => InviteUserHandler);

            services.RemoveAll<IListInvitationsQueryHandler>();
            services.AddScoped(_ => ListInvitationsQueryHandler);
        });
    }

    /// <summary>
    /// Creates a client authenticated as a user in <see cref="TestTenantName"/>
    /// with the given permission codes.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(params string[] permissions)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var header = BuildIdentityHeader(permissions);
        client.DefaultRequestHeaders.Add(TestAuthHandler.IdentityHeader, header);

        return client;
    }

    private static string BuildIdentityHeader(string[] permissions)
    {
        var perms = string.Join(",", permissions);
        return $"{TestTenantId}|{TestTenantName}|{TestUserId}|{perms}";
    }
}
