using Atlas.Identity.Domain.Entities.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Identity.Infrastructure.Persistence.Tenants;

public sealed class UserAuditLogConfig
    : IEntityTypeConfiguration<UserAuditLog>
{
    public void Configure(EntityTypeBuilder<UserAuditLog> b)
    {
        b.ToTable("users_audit");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedNever();

        b.Property(x => x.EntityName)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(x => x.EntityId)
            .HasMaxLength(200);

        b.Property(x => x.UserId)
            .HasMaxLength(200);

        b.Property(x => x.TenantId)
            .IsRequired();

        b.Property(x => x.ChangesJson)
            .IsRequired()
            .HasColumnType("jsonb");

        b.Property(x => x.OccurredAtUtc)
            .IsRequired();

        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.OccurredAtUtc);
        b.HasIndex(x => x.EntityName);
    }
}