using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingsSportsFields.DataAccess.ModelEntity;

public class BookingsConfiguration : IEntityTypeConfiguration<BookingsEntity>
{
    public void Configure(EntityTypeBuilder<BookingsEntity> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.TotalPrice)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.SportsField)
            .WithMany()
            .HasForeignKey(b => b.SportsFieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
