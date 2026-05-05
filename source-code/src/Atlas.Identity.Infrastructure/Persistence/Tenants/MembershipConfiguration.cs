using Atlas.Identity.Domain.Tenants;
using Atlas.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Tenants;

public sealed class MembershipConfig : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> b)
    {
        b.ToTable("memberships");

        b.HasKey(x => x.Id);

        b.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        b.Property(x => x.Role)
            .HasMaxLength(40)
            .HasDefaultValue("User");

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        // 🔹 UserId agora é nullable
        b.Property(x => x.UserId)
            .IsRequired(false);

        // 🔹 FK opcional para IdentityUser
        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Email único por tenant
        b.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique();

        // 🔹 Vínculo único por tenant + identityUser (somente quando não for null)
        b.HasIndex(x => new { x.TenantId, x.UserId })
            .IsUnique()
            .HasFilter("\"UserId\" IS NOT NULL");
    }
}