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

        b.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(80);

        b.HasIndex(x => x.Slug)
            .IsUnique();

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.HasMany(x => x.Memberships)
            .WithOne()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
