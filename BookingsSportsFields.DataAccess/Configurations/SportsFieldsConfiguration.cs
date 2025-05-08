using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace BookingsSportsFields.DataAccess.Configurations
{
    public class SportsFieldsConfiguration : IEntityTypeConfiguration<SportsFieldsEntity>
    {
        public void Configure(EntityTypeBuilder<SportsFieldsEntity> builder)
        {

            builder.ToTable("SportsFields");

            builder.HasKey(sf => sf.Id);

            builder.Property(sf => sf.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(sf => sf.WarningInformation)
                .HasMaxLength(255);

            builder.Property(sf => sf.Type)
                .IsRequired();

            builder.Property(sf => sf.PricePerHour)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(sf => sf.Description)
                .HasMaxLength(1000);

            builder.Property(sf => sf.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(sf => sf.ImageUrl)
                .IsRequired();

            builder.HasOne(sf => sf.Location)
                .WithOne(l => l.SportsFields)
                .HasForeignKey<LocationsEntity>(l => l.SportsFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sf => sf.Owner)
                .WithMany()
                .HasForeignKey(sf => sf.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(sf => sf.Bookings)
                .WithOne(b => b.SportsField)
                .HasForeignKey(b => b.SportsFieldId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
