using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingsSportsFields.DataAccess.ModelEntity;

public class LocationsConfiguration : IEntityTypeConfiguration<LocationsEntity>
{
    public void Configure(EntityTypeBuilder<LocationsEntity> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Address)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasOne(l => l.SportsFields)
            .WithOne(s => s.Location)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(l => l.Latitude)
            .HasPrecision(18, 9); // або інші значення, які підходять для ваших потреб

        builder.Property(l => l.Longitude)
            .HasPrecision(18, 9);
    }
}
