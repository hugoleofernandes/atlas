using Atlas.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

public sealed class TenantConfig : IEntityTypeConfiguration<Tenant>
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
    }
}