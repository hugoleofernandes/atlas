using Atlas.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

public sealed class TenantUserConfig : IEntityTypeConfiguration<TenantUser>
{
    public void Configure(EntityTypeBuilder<TenantUser> b)
    {
        b.ToTable("tenant_users");

        b.HasKey(x => x.Id);

        b.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        b.Property(x => x.Role)
            .HasMaxLength(40)
            .HasDefaultValue("User");

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.HasOne(x => x.Tenant)
            .WithMany(x => x.TenantUsers)
            .HasForeignKey(x => x.TenantId);

        b.HasOne(x => x.User)
            .WithMany(x => x.TenantUsers)
            .HasForeignKey(x => x.UserId);

        // Email único por tenant
        b.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique();

        // Um User só pode ter 1 vínculo por tenant
        b.HasIndex(x => new { x.TenantId, x.UserId })
            .IsUnique();
    }
}