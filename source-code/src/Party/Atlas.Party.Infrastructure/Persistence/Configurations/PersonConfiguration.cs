using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.Party.Contracts.EntityTypes;
using Atlas.Party.Domain.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Party.Infrastructure.Persistence.Configurations;

public sealed class PersonConfiguration : AuditedAggregateConfiguration<Person>
{
    protected override Guid EntityTypeId => PartyModuleEntityTypes.Persons.EntityType.Id;

    protected override void ConfigureEntity(EntityTypeBuilder<Person> b)
    {
        b.OwnsOne(
            x => x.Name,
            n =>
            {
                n.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
                n.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
                n.Property(x => x.MiddleName).HasColumnName("middle_name").HasMaxLength(100);
            }
        );

        b.Property(x => x.BirthDate).HasColumnName("birth_date");

        b.Property(x => x.Gender).HasConversion<string>().HasColumnName("gender").HasMaxLength(20);

        b.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(2000);
    }
}
