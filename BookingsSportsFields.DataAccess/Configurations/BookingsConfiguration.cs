using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingsSportsFields.DataAccess.ModelEntity;

public class BookingsConfiguration : IEntityTypeConfiguration<BookingsEntity>
{
    public void Configure(EntityTypeBuilder<BookingsEntity> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Comment)
            .HasMaxLength(255);

        builder.Property(b => b.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.SportsField)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.SportsFieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
