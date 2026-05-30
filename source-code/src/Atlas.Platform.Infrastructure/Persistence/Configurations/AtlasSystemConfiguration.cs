using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Configurations;
using Atlas.Platform.Domain.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AtlasSystem = Atlas.Platform.Domain.Systems.AtlasSystem;

namespace Atlas.Platform.Infrastructure.Persistence.Configurations;

public sealed class AtlasSystemConfiguration : IEntityTypeConfiguration<AtlasSystem>
{
    public void Configure(EntityTypeBuilder<AtlasSystem> b)
    {
        b.ToTable("systems");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        b.HasIndex(x => x.Name).IsUnique();

        b.Property(x => x.IsActive).IsRequired();

        EntityChangeConfiguration.Configure(b);
    }
}
