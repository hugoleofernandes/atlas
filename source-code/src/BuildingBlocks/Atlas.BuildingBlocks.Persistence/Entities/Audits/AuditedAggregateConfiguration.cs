using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.BuildingBlocks.Persistence.Entities.Audits;

public abstract class AuditedAggregateConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class
{
    protected abstract Guid EntityTypeId { get; }

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasAnnotation(AuditMetadataAnnotations.EntityTypeId, EntityTypeId);
        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
