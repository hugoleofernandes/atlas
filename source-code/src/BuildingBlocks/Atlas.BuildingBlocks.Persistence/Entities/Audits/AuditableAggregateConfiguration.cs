using Atlas.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.BuildingBlocks.Persistence.Entities.Audits;

public abstract class AuditableAggregateConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IAuditableAggregate
{
    protected abstract Guid EntityTypeId { get; }

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasAnnotation(AuditMetadataAnnotations.EntityTypeId, EntityTypeId);
        ConfigureAuditable(builder);
    }

    protected abstract void ConfigureAuditable(EntityTypeBuilder<TEntity> builder);
}
