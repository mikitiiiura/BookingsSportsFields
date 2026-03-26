using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingsSportsFields.DataAccess.Configurations;

public class SportsFieldInstanceConfiguration : IEntityTypeConfiguration<SportsFieldInstanceEntity>
{
    public void Configure(EntityTypeBuilder<SportsFieldInstanceEntity> builder)
    {
        builder.ToTable("SportsFieldInstances");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DisplayName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        // Каскад ТІЛЬКИ на тип спорту — якщо тип видаляється, інстанси теж
        builder.HasOne(e => e.SportType)
            .WithMany(t => t.Instances)
            .HasForeignKey(e => e.SportTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        // На майданчик — без каскаду (щоб уникнути multiple cascade paths)
        builder.HasOne(e => e.SportsField)
            .WithMany(sf => sf.Instances)
            .HasForeignKey(e => e.SportsFieldId)
            .OnDelete(DeleteBehavior.Restrict);   // або .NoAction
    }
}