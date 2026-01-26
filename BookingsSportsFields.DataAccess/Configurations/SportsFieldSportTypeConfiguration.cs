using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingsSportsFields.DataAccess.Configurations;

public class SportsFieldSportTypeConfiguration : IEntityTypeConfiguration<SportsFieldSportTypeEntity>
{
    public void Configure(EntityTypeBuilder<SportsFieldSportTypeEntity> builder)
    {
        builder.ToTable("SportsFieldSportTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type)
            .IsRequired();

        builder.Property(t => t.PricePerHour)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(t => t.WarningInformation)
            .HasMaxLength(255);

        builder.HasOne(t => t.SportsField)
            .WithMany(sf => sf.TypesWithDetails)
            .HasForeignKey(t => t.SportsFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.WeeklySchedules)
            .WithOne(s => s.SportTypeDetail)
            .HasForeignKey(s => s.SportsFieldSportTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}