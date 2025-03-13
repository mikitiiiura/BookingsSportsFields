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
            .WithMany()
            .HasForeignKey(l => l.SportsFieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
