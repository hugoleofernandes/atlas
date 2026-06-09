using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Configurations;
using Atlas.Platform.Contracts.EntityTypes;
using Atlas.Platform.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Platform.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : AuditableAggregateConfiguration<Tenant>
{
    protected override Guid EntityTypeId => PlatformModuleEntityTypes.Tenants.EntityType.Id;

    protected override void ConfigureAuditable(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("tenants");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        b.HasIndex(x => x.Name).IsUnique();

        b.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();

        EntityChangeConfiguration.Configure(b);
    }
}
