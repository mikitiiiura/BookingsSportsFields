using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingsSportsFields.DataAccess.ModelEntity;

public class DiscountsConfiguration : IEntityTypeConfiguration<DiscountsEntity>
{
    public void Configure(EntityTypeBuilder<DiscountsEntity> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.DiscountPercentage)
            .IsRequired();

        builder.Property(d => d.ExpiryDate)
            .IsRequired();
    }
}
