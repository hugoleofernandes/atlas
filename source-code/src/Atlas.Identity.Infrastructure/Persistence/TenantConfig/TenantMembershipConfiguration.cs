using Atlas.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.TenantConfig;

public sealed class TenantMembershipConfig : IEntityTypeConfiguration<TenantMembership>
{
    public void Configure(EntityTypeBuilder<TenantMembership> b)
    {
        b.ToTable("tenant_memberships"); // 🔹 nome ajustado

        b.HasKey(x => x.Id);

        b.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        b.Property(x => x.Role)
            .HasMaxLength(40)
            .HasDefaultValue("User");

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        // 🔹 IdentityUserId agora é nullable
        b.Property(x => x.IdentityUserId)
            .IsRequired(false);

        // 🔹 FK opcional para IdentityUser
        b.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.IdentityUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Email único por tenant
        b.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique();

        // 🔹 Vínculo único por tenant + identityUser (somente quando não for null)
        b.HasIndex(x => new { x.TenantId, x.IdentityUserId })
            .IsUnique()
            .HasFilter("\"IdentityUserId\" IS NOT NULL");
    }
}