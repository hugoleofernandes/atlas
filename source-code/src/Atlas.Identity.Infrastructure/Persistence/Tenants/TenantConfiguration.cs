using Atlas.Identity.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Tenants;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("tenants");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(80);

        b.HasIndex(x => x.Name)
            .IsUnique();

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.Property(x => x.CreatedAt)
            .IsRequired();

        // 🔹 USERS (1:N)
        b.HasMany(x => x.Users)
            .WithOne()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔹 INVITATIONS (1:N)
        b.HasMany(x => x.Invitations)
            .WithOne()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}