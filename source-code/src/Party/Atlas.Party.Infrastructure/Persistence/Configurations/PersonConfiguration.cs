using Atlas.Party.Domain.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Party.Infrastructure.Persistence.Configurations;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> b)
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

