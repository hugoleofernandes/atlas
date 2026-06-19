using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Configurations;
using Atlas.Identity.Contracts.EntityTypes;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : AuditedAggregateConfiguration<User>
{
    protected override Guid EntityTypeId => IdentityModuleEntityTypes.Users.EntityType.Id;

    protected override void ConfigureEntity(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedNever();

        b.Property(x => x.TenantId)
            .IsRequired();

        b.Property(x => x.ExternalId)
            .HasConversion(
                id => id.Value,
                value => ExternalId.Create(value)
            )
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value)
            )
            .HasMaxLength(256)
            .IsRequired();

        b.Property(x => x.RoleId)
            .IsRequired();

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.HasIndex(x => x.ExternalId);
        b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();

        // TenantId is a plain column — no FK constraint (Tenant lives in atlas_platform, cross-module boundary)
        b.Property(x => x.TenantId).IsRequired();

        EntityChangeConfiguration.Configure(b);
    }
}
