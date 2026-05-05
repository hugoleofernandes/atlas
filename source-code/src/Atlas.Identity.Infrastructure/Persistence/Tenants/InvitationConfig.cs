using Atlas.Identity.Domain.Tenants;
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
            .HasMaxLength(256)
            .IsRequired();

        b.Property(x => x.Role)
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.ExpiresAt)
            .IsRequired();

        b.Property(x => x.IsUsed)
            .HasDefaultValue(false);

        b.HasIndex(x => new { x.TenantId, x.Email });
    }
}