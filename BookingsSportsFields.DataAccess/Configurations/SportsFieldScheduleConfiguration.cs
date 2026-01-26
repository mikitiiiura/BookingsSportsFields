using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingsSportsFields.DataAccess.Configurations;

public class SportsFieldScheduleConfiguration : IEntityTypeConfiguration<SportsFieldSchedule>
{
    public void Configure(EntityTypeBuilder<SportsFieldSchedule> builder)
    {
        builder.ToTable("SportsFieldSchedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.DayOfWeek)
            .IsRequired();

        builder.Property(s => s.AvailableFrom)
            .IsRequired();

        builder.Property(s => s.AvailableTo)
            .IsRequired();

        builder.HasOne(s => s.SportTypeDetail)
            .WithMany(t => t.WeeklySchedules)
            .HasForeignKey(s => s.SportsFieldSportTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}