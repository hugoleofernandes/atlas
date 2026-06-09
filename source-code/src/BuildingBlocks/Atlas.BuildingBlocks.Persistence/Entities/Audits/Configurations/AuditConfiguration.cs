using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.BuildingBlocks.Persistence.Entities.Audits.Configurations;

public sealed class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> b)
    {
        b.ToTable("audits");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedNever();

        b.Property(x => x.EntityTypeId)
            .IsRequired();

        b.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(x => x.EntityId)
            .HasMaxLength(200);

        b.Property(x => x.UserId)
            .HasMaxLength(200);

        b.Property(x => x.UserEmail)
            .HasMaxLength(254);

        b.Property(x => x.TenantId)
            .IsRequired();

        b.Property(x => x.ChangesJson)
            .IsRequired()
            .HasColumnType("jsonb");

        b.Property(x => x.OccurredAtUtc)
            .IsRequired();

        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.OccurredAtUtc);
        b.HasIndex(x => x.EntityTypeId);
        b.HasIndex(x => new { x.TenantId, x.EntityTypeId, x.OccurredAtUtc });
        b.HasIndex(x => new { x.TenantId, x.EntityTypeId, x.EntityId });
        b.HasIndex(x => new { x.TenantId, x.EntityTypeId, x.Action, x.OccurredAtUtc });
    }
}
