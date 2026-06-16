using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Configurations;
using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Party.Infrastructure.Persistence.Configurations;

public sealed class PartyConfiguration : IEntityTypeConfiguration<Domain.Parties.Party>
{
    public void Configure(EntityTypeBuilder<Domain.Parties.Party> b)
    {
        b.ToTable("parties");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.TenantId).IsRequired();

        b.Property(x => x.TaxNumber)
            .HasConversion(taxNumber => taxNumber.Value, value => TaxNumber.Create(value))
            .HasColumnName("tax_number")
            .HasMaxLength(14)
            .IsRequired();

        b.Property(x => x.IsActive).HasDefaultValue(true);

        b.HasIndex(x => new { x.TenantId, x.TaxNumber }).IsUnique();

        b.HasDiscriminator<string>("party_type").HasValue<Individual>("Individual").HasValue<Organization>("Organization");

        b.OwnsMany(
            x => x.Addresses,
            a =>
            {
                a.ToTable("party_addresses");

                a.WithOwner().HasForeignKey("PartyId");

                a.HasKey(x => x.Id);

                a.Property(x => x.Id).ValueGeneratedNever();

                a.Property(x => x.Type).HasConversion<string>().HasColumnName("type").HasMaxLength(20).IsRequired();

                a.Property(x => x.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false);

                a.OwnsOne(
                    x => x.PostalAddress,
                    p =>
                    {
                        p.Property(x => x.Street).HasColumnName("street").HasMaxLength(200).IsRequired();
                        p.Property(x => x.Number).HasColumnName("number").HasMaxLength(20).IsRequired();
                        p.Property(x => x.Complement).HasColumnName("complement").HasMaxLength(100);
                        p.Property(x => x.District).HasColumnName("district").HasMaxLength(100).IsRequired();
                        p.Property(x => x.City).HasColumnName("city").HasMaxLength(100).IsRequired();
                        p.Property(x => x.State).HasColumnName("state").HasMaxLength(2).IsRequired();
                        p.Property(x => x.ZipCode).HasColumnName("zip_code").HasMaxLength(8).IsRequired();
                        p.Property(x => x.Country).HasColumnName("country").HasMaxLength(2).IsRequired();
                    }
                );

                a.Property(x => x.CreatedAt).IsRequired();
                a.Property(x => x.CreatedBy);
                a.Property(x => x.CreatedByEmail).HasMaxLength(256);
                a.Property(x => x.UpdatedAt);
                a.Property(x => x.UpdatedBy);
                a.Property(x => x.UpdatedByEmail).HasMaxLength(256);
            }
        );

        b.OwnsMany(
            x => x.Contacts,
            c =>
            {
                c.ToTable("party_contacts");

                c.WithOwner().HasForeignKey("PartyId");

                c.HasKey(x => x.Id);

                c.Property(x => x.Id).ValueGeneratedNever();

                c.Property(x => x.Type).HasConversion<string>().HasColumnName("type").HasMaxLength(20).IsRequired();

                c.Property(x => x.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false);

                c.Property(x => x.Email)
                    .HasConversion(email => email == null ? null : email.Value, value => value == null ? null : EmailAddress.Create(value))
                    .HasColumnName("email")
                    .HasMaxLength(256);

                c.Property(x => x.Phone)
                    .HasConversion(phone => phone == null ? null : phone.Value, value => value == null ? null : PhoneNumber.Create(value))
                    .HasColumnName("phone")
                    .HasMaxLength(20);

                c.Property(x => x.CreatedAt).IsRequired();
                c.Property(x => x.CreatedBy);
                c.Property(x => x.CreatedByEmail).HasMaxLength(256);
                c.Property(x => x.UpdatedAt);
                c.Property(x => x.UpdatedBy);
                c.Property(x => x.UpdatedByEmail).HasMaxLength(256);
            }
        );

        EntityChangeConfiguration.Configure(b);
    }
}
