using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Configurations;
using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Configurations;

public sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> b)
    {
        b.ToTable("invitations");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedNever();

        b.Property(x => x.TenantId)
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

        b.Property(x => x.ExpiresAt)
            .IsRequired();

        b.Property(x => x.IsUsed)
            .HasDefaultValue(false);

        b.HasIndex(x => new { x.TenantId, x.Email });

        b.Ignore(x => x.IsActive);
        b.Ignore(x => x.IsExpired);

        // TenantId is a plain column — no FK constraint (Tenant lives in atlas_platform, cross-module boundary)
        b.Property(x => x.TenantId).IsRequired();

        EntityChangeConfiguration.Configure(b);
    }
}
