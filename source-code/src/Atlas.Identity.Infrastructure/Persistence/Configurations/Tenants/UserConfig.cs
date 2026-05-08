using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Configurations.Tenants;

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
            .HasConversion(
                id => id.Value,
                value => ExternalId.Create(value)
            )
            .HasMaxLength(200)
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

        b.Property(x => x.IsActive)
            .HasDefaultValue(true);

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.HasIndex(x => x.ExternalId);
        b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
    }
}