using Atlas.BuildingBlocks.Persistence.Audits;
using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Entities.Tenants.Configurations;

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

        b.Property(x => x.TenantRoleId)
            .IsRequired();

        b.Property(x => x.ExpiresAt)
            .IsRequired();

        b.Property(x => x.IsUsed)
            .HasDefaultValue(false);

        b.HasIndex(x => new { x.TenantId, x.Email });

        b.Ignore(x => x.IsActive);
        b.Ignore(x => x.IsExpired);

        AuditableEntityConfiguration.Configure(b);
    }
}
