using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Tenants;

public sealed class InvitationConfig : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> b)
    {
        b.ToTable("invitations");

        b.HasKey(x => x.Id);

        b.Property(x => x.TenantId)
            .IsRequired();

        b.Property(x => x.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value)
            )
            .HasMaxLength(256)
            .IsRequired();

        b.Property(x => x.Role)
            .HasConversion(
                role => role.Value,
                value => Role.Create(value)
            )
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.ExpiresAt)
            .IsRequired();

        b.Property(x => x.IsUsed)
            .HasDefaultValue(false);

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.HasIndex(x => new { x.TenantId, x.Email });

        b.Ignore(x => x.IsActive);
        b.Ignore(x => x.IsExpired);
    }
}