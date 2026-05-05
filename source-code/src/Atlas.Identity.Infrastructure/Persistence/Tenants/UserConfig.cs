using Atlas.Identity.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Tenants;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedNever();

        b.Property(x => x.TenantId)
            .IsRequired();

        b.Property(x => x.ExternalId)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        b.Property(x => x.Role)
            .HasMaxLength(50)
            .IsRequired();

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.HasIndex(x => x.ExternalId);
        b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
    }
}