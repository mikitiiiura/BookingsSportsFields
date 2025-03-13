using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingsSportsFields.DataAccess.ModelEntity;

public class BookingDiscountsConfiguration : IEntityTypeConfiguration<BookingDiscountsEntity>
{
    public void Configure(EntityTypeBuilder<BookingDiscountsEntity> builder)
    {
        builder.HasKey(bd => bd.Id);

        builder.Property(bd => bd.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(bd => bd.BookingName)
            .WithMany()
            .HasForeignKey(bd => bd.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bd => bd.Discount)
            .WithMany()
            .HasForeignKey(bd => bd.DiscountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
